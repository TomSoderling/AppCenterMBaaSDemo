using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        //private User lastSelectedUser;

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

        public ICommand CreateNewContactCommand { get; private set; }
        public ICommand GetUserDocumentsCommand { get; private set; }
        public ICommand GetUserDocumentCommand { get; private set; }
        public ICommand UpsertContactCommand { get; private set; }
        public ICommand DeleteContactCommand { get; private set; }

        public ObservableCollection<Contact> Contacts { get; set; } = new ObservableCollection<Contact>();


        private async Task CreateNewContact()
        {
            StatusMessage = string.Empty;

            //var user = new User(Name, Email, PhoneNumber);

            //try
            //{
            //    // Optional time-to-live parameter. Locally cached documents will expire afte 60 seconds.
            //    var ttl = new TimeSpan(0, 0, 60, 0);

            //    var doc = await Data.CreateAsync(user.Id.ToString(), user, DefaultPartitions.UserDocuments, new WriteOptions(ttl));

            //    if (doc.IsFromDeviceCache)
            //    {
            //        StatusMessage = $"User created in device cache. TTL: {ttl.Seconds} seconds. ID: {doc.Id}";
            //    }
            //    else
            //    {
            //        StatusMessage = $"User created in App Center backend. ID: {doc.Id}";
            //    }

            //    Name = Email = PhoneNumber = string.Empty;
            //}
            //catch (Exception ex)
            //{
            //    StatusMessage = ex.Message;
            //}

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

        private async Task GetContactByID(Contact contact)
        {
            //StatusMessage = string.Empty;
            //Name = Email = PhoneNumber = string.Empty;

            //try
            //{
            //    var fetchedUser = await Data.ReadAsync<User>(user.Id.ToString(), DefaultPartitions.UserDocuments);

            //    StatusMessage = $"User fetched from {CacheOrService(fetchedUser)}: \n{fetchedUser.DeserializedValue.Id} \n{fetchedUser.LastUpdatedDate.ToLocalTime()}";

            //    // populate text fields so the values can be changed
            //    lastSelectedUser = fetchedUser.DeserializedValue;
            //    Name = fetchedUser.DeserializedValue.Name;
            //    Email = fetchedUser.DeserializedValue.Email;
            //    PhoneNumber = fetchedUser.DeserializedValue.PhoneNumber;
            //}
            //catch (DataException dex)
            //{
            //    StatusMessage = dex.InnerException?.Message;
            //}
            //catch (Exception ex)
            //{
            //    StatusMessage = ex.Message;
            //}
        }

        private async Task GetListOfUserDocuments()
        {
            //StatusMessage = string.Empty;
            //Name = Email = PhoneNumber = string.Empty;

            //try
            //{
            //    var paginatedDocs = await Data.ListAsync<User>(DefaultPartitions.UserDocuments); // get the currently authenticated user's documents

            //    UserDocuments.Clear();
            //    var pageOfDocs = new List<User>();
            //    pageOfDocs.AddRange(paginatedDocs.CurrentPage.Items.Select(d => d.DeserializedValue));

            //    // Add to ObservableCollection
            //    foreach (var user in pageOfDocs)
            //        UserDocuments.Add(user);

            //    StatusMessage = $"Documents fetched from {CacheOrService(paginatedDocs.First())}";
            //}
            //catch (DataException dex)
            //{
            //    StatusMessage = dex.InnerException?.Message;
            //}
            //catch (Exception ex)
            //{
            //    StatusMessage = ex.Message;
            //}


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

                    while (queryResultSetIterator.HasMoreResults)
                    {
                        var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (var doc in currentResultSet)
                        {
                            Contacts.Add(doc);
                        }
                    }

                    StatusMessage = "Documents fetched";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task UpsertContect()
        {
            //StatusMessage = string.Empty;

            //try
            //{
            //    // update any values the user may have changed
            //    lastSelectedUser.Name = Name;
            //    lastSelectedUser.Email = Email;
            //    lastSelectedUser.PhoneNumber = PhoneNumber;

            //    var upsertedUser = await Data.ReplaceAsync<User>(lastSelectedUser.Id.ToString(), lastSelectedUser, DefaultPartitions.UserDocuments);

            //    StatusMessage = $"User upserted to {CacheOrService(upsertedUser)} \n{upsertedUser.DeserializedValue.Id} \n{upsertedUser.LastUpdatedDate.ToLocalTime()}";

            //    Name = Email = PhoneNumber = string.Empty;
            //}
            //catch (DataException dex)
            //{
            //    StatusMessage = dex.InnerException?.Message;
            //}
            //catch (Exception ex)
            //{
            //    StatusMessage = ex.Message;
            //}
        }

        private async Task DeleteContact(Contact selectedContact)
        {
            //StatusMessage = string.Empty;

            //try
            //{
            //    var result = await Data.DeleteAsync<User>(selectedUser.Id.ToString(), DefaultPartitions.UserDocuments);

            //    StatusMessage = $"User deleted in {CacheOrService(result)}";
            //    Name = Email = PhoneNumber = string.Empty;
            //}
            //catch (DataException dex)
            //{
            //    StatusMessage = dex.InnerException?.Message;
            //}
            //catch (Exception ex)
            //{
            //    StatusMessage = ex.Message;
            //}
        }

        //private string CacheOrService(DocumentWrapper<User> userDoc)
        //{
        //    return userDoc.IsFromDeviceCache ? "device cache" : "App Center backend";
        //}


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
