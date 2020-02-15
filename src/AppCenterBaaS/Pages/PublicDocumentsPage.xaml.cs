using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace AppCenterBaaS.Pages
{
    public partial class PublicDocumentsPage : ContentPage
    {
        public PublicDocumentsPage()
        {
            InitializeComponent();
        }

        void ListView_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}
