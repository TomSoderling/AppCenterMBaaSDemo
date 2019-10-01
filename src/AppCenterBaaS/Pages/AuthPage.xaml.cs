using System.ComponentModel;
using AppCenterBaaS.ViewModels;
using Xamarin.Forms;

namespace AppCenterBaaS.Pages
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class AuthPage : ContentPage
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((AuthPageViewModel)this.BindingContext).SetUserIDCommand.Execute(null);
        }
    }
}
