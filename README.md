# App Center MBaaS Demo

Demo Xamarin.Forms app showing the power of Visual Studio App Center Data & Auth to build a Mobile Backend as a Service (MBaaS)

See repo for slides that go along with the app



### Using this sample app
You'll need to provide your own App Center app secrets. You generate them by creating new iOS and/or Android apps in App Center. Once you have them:  
 - Add your secrets to the top of the App.xaml.cs which will be used in the AppCenter.Start() call
 - Add your iOS app secret to the CFBundleURLTypes array in the Info.plist file:  
 ```
<key>CFBundleURLSchemes</key>
 <array>
  <string>msal9ebe1304-7b23-4d19-abb6-3dfc71ceed4d</string>
 </array>
```
 - Add your Android app secret to the this string in the AndroidManifest.xml file, intent-filter node:  
`<data android:host="auth" android:scheme="msal[your guid goes here]" />`
 
