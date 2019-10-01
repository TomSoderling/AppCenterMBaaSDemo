# App Center MBaaS Demo

Demo Xamarin.Forms app showing the power of Visual Studio App Center Data & Auth to build a Mobile Backend as a Service (MBaaS)

See repo for slides that go along with the app



### Using this sample app
You'll need to provide your own app secrets. You can generate them by creating new iOS and/or Android apps in App Center. Once you have them:  
 - Add your secrets to the top of the App.xaml.cs which will be used in the AppCenter.Start() call
 - Add your iOS app secret to the "msal[guid goes here]" string in the Info.plist file
 - Add your Android app secret to the "msal[guid goes here]" string in the AndroidManifest.xml file
 
