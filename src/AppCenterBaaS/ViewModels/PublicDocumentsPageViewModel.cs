using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AppCenterBaaS.Models;
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


        public const string accountURL = @"https://cosmos-db-demo-app-1.documents.azure.com:443/";
        // This is the master key. In practice, you would NOT want to bundle this with your app and give every user this level of access
        public const string accountKey = "TNKoJ4biqCXWWC1jjoRlrM4c056t5M5oKDpzkVRRvwyTBhXcdW701lZ31PSvV6GFTjucqc0hgvxaMg0OMWW7yg==";
        public const string databaseId = "DemoAppDB";
        public const string containerId = "Contacts";
        public const string partitionKey = "/userId";

        private async Task GetAllPublicAppDocuments()
        {
            StatusMessage = "Fetching documents...";
            JsonDoc = string.Empty;

            try
            {
                // Create new CosmosClient to communiciate with Azure Cosmos DB
                using (var cosmosClient = new CosmosClient(accountURL, accountKey))
                {
                    // Connect to or create database
                    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

                    // Connect to or create container
                    var container = await database.Database.CreateContainerIfNotExistsAsync(containerId, partitionKey);

                    var sqlQueryText = "SELECT * FROM c";
                    var queryDefinition = new QueryDefinition(sqlQueryText);
                    var queryResultSetIterator = container.Container.GetItemQueryIterator<Contact>(queryDefinition);

                    if (queryResultSetIterator.HasMoreResults)
                        PublicDocuments.Clear();

                    while (queryResultSetIterator.HasMoreResults)
                    {
                        var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (var doc in currentResultSet)
                        {
                            var lastUpdated = $"last updated: {doc.LastUpdated}";
                            PublicDocuments.Add(new Tuple<string, string>(doc.Id.ToString(), lastUpdated));
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

        private async Task GetPublicAppDocumentByID(object selected)
        {
            if (!(selected is Tuple<string, string> selectedDoc))
                return;

            StatusMessage = $"Fetching document ...{selectedDoc.Item1.Substring(selectedDoc.Item1.Length - 5, 5)}...";

            try
            {
                // Create new CosmosClient to communiciate with Azure Cosmos DB
                using (var cosmosClient = new CosmosClient(accountURL, accountKey))
                {
                    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                    var container = await database.Database.CreateContainerIfNotExistsAsync(containerId, partitionKey);

                    var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{selectedDoc.Item1}'";
                    var query = new QueryDefinition(sqlQueryText);
                    var queryResultSetIterator = container.Container.GetItemQueryStreamIterator(query);

                    if (queryResultSetIterator.HasMoreResults)
                    {
                        var responseMessage = await queryResultSetIterator.ReadNextAsync();

                        var docAsJson = string.Empty;
                        using (StreamReader sr = new StreamReader(responseMessage.Content))
                        {
                            docAsJson = sr.ReadToEnd();
                        }

                        // format as json for display
                        var parsedJson = JToken.Parse(docAsJson);
                        JsonDoc = parsedJson.ToString(Formatting.Indented);
                    }

                    StatusMessage = $"Document ...{selectedDoc.Item1.Substring(selectedDoc.Item1.Length - 5, 5)} fetched";
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
