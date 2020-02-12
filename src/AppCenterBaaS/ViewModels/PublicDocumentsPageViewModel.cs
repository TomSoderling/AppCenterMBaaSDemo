using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace AppCenterBaaS.ViewModels
{
    public class PublicDocumentsPageViewModel : INotifyPropertyChanged
    {
        public PublicDocumentsPageViewModel()
        {
            GetAllPublicAppDocumentsCommand = new Command(async () => await GetAllPublicAppDocuments());
            GetPublicAppDocumentCommand = new Command(async (object doc) => await GetPublicAppDocumentByID(doc));
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

        public ICommand GetAllPublicAppDocumentsCommand { get; private set; }
        public ICommand GetPublicAppDocumentCommand { get; private set; }

        public ObservableCollection<Tuple<string, string>> PublicDocuments { get; set; } = new ObservableCollection<Tuple<string, string>>();


        const string accountURL = @"https://cosmos-db-demo-app-1.documents.azure.com:443/";
        const string accountKey = @"TNKoJ4biqCXWWC1jjoRlrM4c056t5M5oKDpzkVRRvwyTBhXcdW701lZ31PSvV6GFTjucqc0hgvxaMg0OMWW7yg==";
        const string databaseId = @"ToDoList";
        const string collectionId = @"Items";
        public CosmosClient client;

        private async Task GetAllPublicAppDocuments()
        {
            //StatusMessage = string.Empty;
            //JsonDoc = string.Empty;

            //try
            //{
            //    var paginatedDocs = await Data.ListAsync<object>(DefaultPartitions.AppDocuments);

            //    PublicDocuments.Clear();
            //    foreach (var doc in paginatedDocs)
            //    {
            //        var lastUpdated = $"last updated: {doc.LastUpdatedDate.ToLocalTime().ToString()}";
            //        PublicDocuments.Add(new Tuple<string, string>(doc.Id, lastUpdated));
            //    }

            //    StatusMessage = "Documents fetched";
            //}
            //catch (Exception ex)
            //{
            //    StatusMessage = ex.Message;
            //}


            try
            {
                // Create new CosmosClient to communiciate with Azure Cosmos DB
                using (var cosmosClient = new CosmosClient(accountURL, accountKey))
                {
                    // Create new database
                    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

                    // Create new container
                    var container = await database.Database.CreateContainerIfNotExistsAsync(collectionId, "/_partitionKey");

                    var sqlQueryText = "SELECT * FROM c";
                    var queryDefinition = new QueryDefinition(sqlQueryText);
                    var queryResultSetIterator = container.Container.GetItemQueryIterator<TodoItem>(queryDefinition);

                    var items = new List<TodoItem>();

                    while (queryResultSetIterator.HasMoreResults)
                    {
                        var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (TodoItem item in currentResultSet)
                        {
                            items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task GetPublicAppDocumentByID(object selected)
        {
            //StatusMessage = string.Empty;

            //if (!(selected is Tuple<string, string> selectedDoc))
            //    return;

            //try
            //{
            //    var doc = await Data.ReadAsync<object>(selectedDoc.Item1, DefaultPartitions.AppDocuments);

            //    // format the json for display
            //    var parsedJson = JToken.Parse(doc.JsonValue);
            //    JsonDoc = parsedJson.ToString(Formatting.Indented);

            //    var cacheOrService = doc.IsFromDeviceCache ? "device cache" : "App Center backend";
            //    StatusMessage = $"Document fetched from {cacheOrService}";
            //}
            //catch (Exception ex)
            //{
            //    StatusMessage = ex.Message;
            //}
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public class TodoItem
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "text")]
            public string Text { get; set; }

            [JsonProperty(PropertyName = "complete")]
            public bool Complete { get; set; }

            [JsonProperty(PropertyName = "userid")]
            public string UserId;
        }
    }
}
