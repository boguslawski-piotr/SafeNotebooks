using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SafeNotebooks
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class App : Application
	{
		//

		public static Data Data;


		//

		public App()
		{
			Data = new Data();
			InitializeComponent();
			MainPage = new MainFrame();
		}

		private Unlock __unlock = null;
		private Unlock _unlock
		{
			get
			{
				if (__unlock == null)
					__unlock = new Unlock();
				return __unlock;
			}
		}

		async protected override void OnStart()
		{
			//MainPage.Navigation.PushModalAsync(new Settings());
			Debug.WriteLine("OnStart");

			// TODO: create credentials manager

			// TODO: pass credentials manager to data


			// TODO: if user want to: ask for MP/pin/biometrics
			//_unlock.UnlockMode();
			//MainPage.Navigation.PushModalAsync(_unlock, false);

			// TODO: prepare available FileSystems (with logins)

			// TODO: pass FileSystems to data


			// TODO: load data (minimum set -> global data settings, list of notebooks (minimum data))

			// TODO: restore last selections (with unlocking if necessary)
		}

		async protected override void OnSleep()
		{
			// Handle when your app sleeps
			Debug.WriteLine("OnSleep");

			// TODO: if user want to: lock all data and clear all forms (unselect)
			//Data.SelectNotebook(null, false);
			//Data.SelectPage(null, false);

			// Show lock screen in order to hide data
			//_unlock.SplashMode();
			MainPage.Navigation.PushModalAsync(_unlock, false);
			await Task.Delay(5000);
		}

		async protected override void OnResume()
		{
			// Handle when your app resumes
			Debug.WriteLine("OnResume");

			// TODO: if user want to: ask for MP/pin/biometrics
			//_unlock.UnlockMode();
			// or
			// TODO: dispose lock screen
			await MainPage.Navigation.PopModalAsync();

			// TODO: if not was locked in OnSleep restore previously selected data
		}
	}
}
