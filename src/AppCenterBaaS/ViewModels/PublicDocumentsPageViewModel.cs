using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
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
    }
}
