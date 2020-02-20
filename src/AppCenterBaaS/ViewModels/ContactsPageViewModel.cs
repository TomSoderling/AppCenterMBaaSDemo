using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AppCenterBaaS.Models;
using Microsoft.Azure.Cosmos;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AppCenterBaaS.ViewModels
{
    public class ContactsPageViewModel : INotifyPropertyChanged
    {
        public ContactsPageViewModel()
        {
            GetUserDocumentsCommand = new Command(async () => await GetListOfUserDocuments());
            GetUserDocumentCommand = new Command<Contact>(async (Contact contact) => await GetContactByID(contact));
            CreateNewContactCommand = new Command(async () => await CreateNewContact());
            UpsertContactCommand = new Command(async () => await UpsertContect());
            DeleteContactCommand = new Command<Contact>(async (Contact contact) => await DeleteContact(contact));


            // Get notified of a pending operation being executed when the device goes from offline to online
            //Data.RemoteOperationCompleted += (sender, eventArgs) =>
            //{
            //    var msg = $"Remote operation completed. {eventArgs.Operation} on {eventArgs.DocumentMetadata.Id}";

            //    if (eventArgs.Error != null)
            //        msg += $". Error: {eventArgs.Error.Message}";

            //    StatusMessage = msg;
            //};
        }

        private Contact lastSelectedContact;


        #region Properties

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        private string email;
        public string Email
        {
            get => email;
            set
            {
                email = value;
                NotifyPropertyChanged();
            }
        }

        private string phoneNumber;
        public string PhoneNumber
        {
            get => phoneNumber;
            set
            {
                phoneNumber = value;
                NotifyPropertyChanged();
            }
        }

        private string notes;
        public string Notes
        {
            get => notes;
            set
            {
                notes = value;
                NotifyPropertyChanged();
            }
        }

        private string statusMessage;
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                NotifyPropertyChanged();
            }
        }

        private string jsonDoc;
        public string JsonDoc
        {
            get => jsonDoc;
            set
            {
                jsonDoc = value;
                NotifyPropertyChanged();
            }
        }


        public ICommand CreateNewContactCommand { get; private set; }
        public ICommand GetUserDocumentsCommand { get; private set; }
        public ICommand GetUserDocumentCommand { get; private set; }
        public ICommand UpsertContactCommand { get; private set; }
        public ICommand DeleteContactCommand { get; private set; }

        public ObservableCollection<Contact> Contacts { get; set; } = new ObservableCollection<Contact>();

        #endregion Properties


        private async Task GetListOfUserDocuments()
        {
            StatusMessage = "Fetching documents...";

            try
            {
                // Create new CosmosClient to communiciate with Azure Cosmos DB
                using (var cosmosClient = new CosmosClient(PublicDocumentsPageViewModel.accountURL, PublicDocumentsPageViewModel.accountKey))
                {
                    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(PublicDocumentsPageViewModel.databaseId);
                    var container = await database.Database.CreateContainerIfNotExistsAsync(PublicDocumentsPageViewModel.containerId, PublicDocumentsPageViewModel.partitionKey);

                    var sqlQueryText = "SELECT * FROM c";
                    var queryDefinition = new QueryDefinition(sqlQueryText);
                    var queryResultSetIterator = container.Container.GetItemQueryIterator<Contact>(queryDefinition);

                    if (queryResultSetIterator.HasMoreResults)
                        Contacts.Clear();

                    var accountID = Preferences.Get("accountID", string.Empty); // get account ID from app preferences

                    while (queryResultSetIterator.HasMoreResults)
                    {
                        var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (var doc in currentResultSet)
                        {
                            if (doc.UserId == accountID) // TODO: shouldn't have to filter these. Use resource token to just get the user's documents.
                                Contacts.Add(doc);
                        }
                    }

                    StatusMessage = "Documents fetched";
                    Name = Email = PhoneNumber = Notes = string.Empty;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task GetContactByID(Contact contact)
        {
            StatusMessage = $"Fetching document ...{contact.Id.ToString().Substring(contact.Id.ToString().Length - 5, 5)}...";

            try
            {
                // Create new CosmosClient to communiciate with Azure Cosmos DB
                using (var cosmosClient = new CosmosClient(PublicDocumentsPageViewModel.accountURL, PublicDocumentsPageViewModel.accountKey))
                {
                    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(PublicDocumentsPageViewModel.databaseId);
                    var container = await database.Database.CreateContainerIfNotExistsAsync(PublicDocumentsPageViewModel.containerId, PublicDocumentsPageViewModel.partitionKey);

                    var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{contact.Id}'";
                    var query = new QueryDefinition(sqlQueryText);
                    var queryResultSetIterator = container.Container.GetItemQueryIterator<Contact>(query);

                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<Contact> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                        var selectedContact = currentResultSet.First();
                        
                        // populate text fields so the values can be changed
                        Name = selectedContact.Name;
                        Email = selectedContact.Email;
                        PhoneNumber = selectedContact.PhoneNumber;
                        Notes = selectedContact.Notes;

                        lastSelectedContact = selectedContact;
                    }

                    StatusMessage = $"Document ...{contact.Id.ToString().Substring(contact.Id.ToString().Length - 5, 5)} fetched";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task CreateNewContact()
        {
            StatusMessage = string.Empty;

            var newContact = new Contact
            {
                Name = Name,
                Email = Email,
                PhoneNumber = PhoneNumber,
                Notes = Notes
            };

            try
            {
                using (var cosmosClient = new CosmosClient(PublicDocumentsPageViewModel.accountURL, PublicDocumentsPageViewModel.accountKey))
                {
                    var databaseResp = await cosmosClient.CreateDatabaseIfNotExistsAsync(PublicDocumentsPageViewModel.databaseId);
                    var containerResp = await databaseResp.Database.CreateContainerIfNotExistsAsync(PublicDocumentsPageViewModel.containerId, PublicDocumentsPageViewModel.partitionKey);

                    var accountID = Preferences.Get("accountID", string.Empty); // get account ID from app preferences
                    newContact.UserId = accountID;

                    var response = await containerResp.Container.CreateItemAsync(newContact, new PartitionKey(accountID));

                    StatusMessage = $"Document created with ID: {response.Resource.Id}. Operation consumed {response.RequestCharge} RUs";
                }
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                    StatusMessage = $"Document in database with ID: {newContact.Id} already exists";
                else
                    StatusMessage = $"Error creating document: {ex.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating document: {ex.Message}";
            }
        }

        private async Task UpsertContect()
        {
            StatusMessage = $"Upserting document ...{lastSelectedContact.Id.ToString().Substring(lastSelectedContact.Id.ToString().Length - 5, 5)}...";

            // update any values the user may have changed
            lastSelectedContact.Name = Name;
            lastSelectedContact.Email = Email;
            lastSelectedContact.PhoneNumber = PhoneNumber;
            lastSelectedContact.Notes = Notes;

            try
            {
                // Create new CosmosClient to communiciate with Azure Cosmos DB
                using (var cosmosClient = new CosmosClient(PublicDocumentsPageViewModel.accountURL, PublicDocumentsPageViewModel.accountKey))
                {
                    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(PublicDocumentsPageViewModel.databaseId);
                    var container = await database.Database.CreateContainerIfNotExistsAsync(PublicDocumentsPageViewModel.containerId, PublicDocumentsPageViewModel.partitionKey);

                    var itemResponse = await container.Container.UpsertItemAsync<Contact>(lastSelectedContact, new PartitionKey(lastSelectedContact.UserId));

                    if (itemResponse.StatusCode == HttpStatusCode.OK)
                        StatusMessage = $"Document ...{lastSelectedContact.Id.ToString().Substring(lastSelectedContact.Id.ToString().Length - 5, 5)} upserted. Operation consumed {itemResponse.RequestCharge} RUs";
                    else
                        statusMessage = $"Something went wrong upserting document {lastSelectedContact.Id}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task DeleteContact(Contact selectedContact)
        {
            StatusMessage = $"Deleting document ...{selectedContact.Id.ToString().Substring(selectedContact.Id.ToString().Length - 5, 5)}...";

            try
            {
                // Create new CosmosClient to communiciate with Azure Cosmos DB
                using (var cosmosClient = new CosmosClient(PublicDocumentsPageViewModel.accountURL, PublicDocumentsPageViewModel.accountKey))
                {
                    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(PublicDocumentsPageViewModel.databaseId);
                    var container = await database.Database.CreateContainerIfNotExistsAsync(PublicDocumentsPageViewModel.containerId, PublicDocumentsPageViewModel.partitionKey);

                    var itemResponse = await container.Container.DeleteItemAsync<Contact>(selectedContact.Id.ToString(), new PartitionKey(selectedContact.UserId));

                    if (itemResponse.StatusCode == HttpStatusCode.NoContent)
                    {
                        StatusMessage = $"Document ...{selectedContact.Id.ToString().Substring(selectedContact.Id.ToString().Length - 5, 5)} deleted. Operation consumed {itemResponse.RequestCharge} RUs";
                        Name = Email = PhoneNumber = Notes = string.Empty;
                    }
                    else
                        statusMessage = $"Something went wrong deleting document {selectedContact.Id}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
