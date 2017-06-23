using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using pbXNet;

namespace SafeNotebooks.Droid
{
	[Activity(Label = "@string/app_name", Icon = "@drawable/ic_launcher", Theme = "@style/MyTheme", MainLauncher = true,
			  ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
#if !DEBUG
			pbXNet.Log.AddLogger(new pbXNet.AndroidUtilLogLogger(Title));
#endif
			base.OnCreate(bundle);

			SecretsManager.MainActivity = this;

			global::Xamarin.Forms.Forms.Init(this, bundle);

			LoadApplication(new App());
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (SecretsManager.OnActivityResult(requestCode, resultCode, data))
				return;

			base.OnActivityResult(requestCode, resultCode, data);
		}
	}
}
