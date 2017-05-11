using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class App : Application
	{
		public static string MsgNavDrawerVisibilityChanged = "MsgNavDrawerVisibilityChanged";
		public static string MsgChangeNavDrawerVisibility = "MsgChangeNavDrawerVisibility";

		public App()
		{
			InitializeComponent();
			MainPage = new MainFrame();
		}

		protected override void OnStart()
		{
			// Handle when your app starts
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
