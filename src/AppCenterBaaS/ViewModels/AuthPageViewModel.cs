using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AppCenterBaaS.ViewModels
{
    public class AuthPageViewModel : INotifyPropertyChanged
    {
        public AuthPageViewModel()
        {
            SignInCommand = new Command(async () => await SignIn());
            SignOutCommand = new Command(async () => await SignOut());
            CrashCommand = new Command(() => GenerateTestCrash());
            ErrorCommand = new Command(() => GenerateTestError());

            LoadInfoFromDevicePreferences();
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

        private string accountID;
        public string AccountID
        {
            get => accountID;
            set
            {
                accountID = value;
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

        private string authtime;
        public string AuthTime
        {
            get => authtime;
            set
            {
                authtime = value;
                NotifyPropertyChanged();
            }
        }

        private string expires;
        public string Expires
        {
            get => expires;
            set
            {
                expires = value;
                NotifyPropertyChanged();
            }
        }


        public ICommand SignInCommand { get; private set; }
        public ICommand SignOutCommand { get; private set; }
        public ICommand CrashCommand { get; private set; }
        public ICommand ErrorCommand { get; private set; }


        private async Task SignIn()
        {
            StatusMessage = string.Empty;

            try
            {
                //UserInformation userInfo = await Auth.SignInAsync();

                AuthenticationResult authResult = null;
                var accounts = await App.PCA.GetAccountsAsync(); // get any avail. accounts in the user token cache for this app

                try
                {
                    // First, try to use a previously cached token. Once MSAL.NET has acquired a user token, it caches it. Next time the application wants a token,
                    // it should first call AcquireTokenSilent() to verify if an acceptable token is in the cache, can be refreshed, or can get derived.
                    authResult = await App.PCA.AcquireTokenSilent(App.Scopes, accounts.FirstOrDefault()).ExecuteAsync();
                }
                catch (MsalUiRequiredException ex)
                {
                    try
                    {
                        // Log in interactively. Gets the token through an interactive process that prompts the user for credentials through a browser or pop-up window
                        authResult = await App.PCA.AcquireTokenInteractive(App.Scopes).
                                                    WithAccount(GetAccountByPolicy(accounts, App.PolicySignUpSignIn)).
                                                    WithParentActivityOrWindow(App.ParentWindow).
                                                    ExecuteAsync();
                    }
                    catch (Exception ex2)
                    {
                        StatusMessage = $"Acquire token interactive failed. See exception message for details: {ex2.Message}";
                    }
                }

                if (authResult != null)
                {
                    StatusMessage = $"Signed in! 🎉";

                    AccountID = authResult.UniqueId;
                    ReadClaimsFromJWTToken(authResult.IdToken);

                    SaveInfoToDevicePreferences();
                    
                    AppCenter.SetUserId(Email); // Set user ID to correlate crashes and errors to this authenticated user 😎
                }
            }
            catch (Exception ex)
            {
                // Do something with sign-in failure
                StatusMessage = ex.Message;
            }
        }

        private async Task SignOut()
        {
            StatusMessage = string.Empty;

            try
            {
                //Auth.SignOut();

                var accounts = await App.PCA.GetAccountsAsync();

                while (accounts.Any())
                {
                    await App.PCA.RemoveAsync(accounts.FirstOrDefault());
                    accounts = await App.PCA.GetAccountsAsync();
                }

                StatusMessage = $"Signed out";
                AccountID = Email = Name = AuthTime = Expires = string.Empty;
                Preferences.Clear();

                AppCenter.SetUserId(string.Empty); // Reset user ID to blacnk since we don't have one to correlate crashes and errors to
            }
            catch (Exception ex)
            {
                // Do something with sign out failure
                StatusMessage = ex.Message;
            }
        }


        private void GenerateTestCrash()
        {
            Crashes.GenerateTestCrash();
        }

        private void GenerateTestError()
        {
            var moreInfo = new Dictionary<string, string>
            {
                { "additionalInfo", "More info about this handled exception or error goes here" }
            };

            Crashes.TrackError(new NullReferenceException("Test NRE"), moreInfo);

            StatusMessage = $"Test error generated at {DateTime.Now}";
        }

        // Decode the raw token string to read the claims
        // Claims are values about the user returned to the application in the token.
        // These are enabled in Azure AD B2C > User flow > Application claims
        private void ReadClaimsFromJWTToken(string IdToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwToken = tokenHandler.ReadJwtToken(IdToken);

                // Get display name
                var displayName = jwToken.Claims.FirstOrDefault(t => t.Type == "name")?.Value;
                if (displayName != null)
                {
                    Name = displayName;
                }

                // Get first email address
                var firstEmail = jwToken.Claims.FirstOrDefault(t => t.Type == "emails")?.Value;
                if (firstEmail != null)
                {
                    Email = firstEmail;
                }

                // Get auth time
                var authTime = jwToken.Claims.FirstOrDefault(t => t.Type == "auth_time")?.Value;
                if (authTime != null)
                {
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    AuthTime = epoch.AddSeconds(Convert.ToDouble(authTime)).ToLocalTime().ToString();
                }

                // Get expiration date
                var exp = jwToken.Claims.FirstOrDefault(t => t.Type == "exp")?.Value;
                if (exp != null)
                {
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    Expires = epoch.AddSeconds(Convert.ToDouble(exp)).ToLocalTime().ToString();
                }
            }
            catch (ArgumentException)
            {
                StatusMessage += "\nUnable to get profile info from JWT";
            }
        }

        private void SaveInfoToDevicePreferences()
        {
            Preferences.Set("accountID", AccountID);
            Preferences.Set("displayName", Name);
            Preferences.Set("email", Email);
            Preferences.Set("authTime", AuthTime);
            Preferences.Set("expires", Expires);
        }

        private void LoadInfoFromDevicePreferences()
        {
            AccountID = Preferences.Get("accountID", string.Empty);
            Name = Preferences.Get("displayName", string.Empty);
            Email = Preferences.Get("email", string.Empty);
            AuthTime = Preferences.Get("authTime", string.Empty);
            Expires = Preferences.Get("expires", string.Empty);
        }

        private IAccount GetAccountByPolicy(IEnumerable<IAccount> accounts, string policy)
        {
            foreach (var account in accounts)
            {
                string userIdentifier = account.HomeAccountId.ObjectId.Split('.')[0];

                if (userIdentifier.EndsWith(policy.ToLower()))
                    return account;
            }

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
