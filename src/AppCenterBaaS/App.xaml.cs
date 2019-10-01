using AppCenterBaaS.Pages;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Auth;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Data;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppCenterBaaS
{
    public partial class App : Application
    {
        #region secrets... sh...
        const string iOSSecret = "01234567-nota-real-guid-75c145568e90";
        const string AndroidSecret = "76543210-nota-real-guid-75c145568e90";
        #endregion secrets... sh...

        public App()
        {
            InitializeComponent();

            MainPage = new MainTabbedPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts

            AppCenter.Start($"ios={iOSSecret};android={AndroidSecret}",
                  typeof(Analytics),
                  typeof(Crashes),
                  typeof(Auth),
                  typeof(Data));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
