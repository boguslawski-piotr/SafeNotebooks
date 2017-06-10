using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SafeNotebooks.Droid
{
	[Activity(Label = "@string/app_name", Icon = "@drawable/ic_launcher", Theme = "@style/MyTheme", MainLauncher = true,
			  ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		public static MainActivity Current;

		public MainActivity()
		{
			Current = this;
		}

		protected override void OnCreate(Bundle bundle)
		{
#if !DEBUG
			pbXNet.Log.AddLogger(new pbXNet.AndroidUtilLogLogger(Title));
#endif
			//TabLayoutResource = Resource.Layout.Tabbar;
			//ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
        }
    }
}
