using System.Threading.Tasks;
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
            InitializeComponent();


			// TODO: list of available FileSystems

			// TODO: create credentials manager

			Data = new Data();

			// TODO: pass credentials manager to data

			// TODO: pass FileSystems to data


			MainPage = new MainFrame();


			// TODO: load data (minimum set -> global data settings, list of notebooks (minimum data))

			// TODO: restore last selections (with unlocking if necessary)
		}

		protected override void OnStart()
		{
			// Handle when your app starts
			MainPage.Navigation.PushModalAsync(new Settings());
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
