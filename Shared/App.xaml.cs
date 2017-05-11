using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class App : Application
	{
		//

		public static Data Data;


		//

		public App()
		{
			// TODO: list of available FileSystems

			// TODO: credentials manager

			Data = new Data();

			InitializeComponent();

			MainPage = new MainFrame();

			// TODO: restore last selected data
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
