using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Plugin.Settings;
using Plugin.Settings.Abstractions;

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SafeNotebooks
{
	//[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class App : Application
	{
		//

		static Lazy<Data> _data = new Lazy<Data>(() => new Data(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
		public static Data Data
		{
			get { return _data.Value; }
		}

		//

		public static ISettings Settings
		{
			get { return CrossSettings.Current; }
		}

		//

		public App()
		{
			InitializeComponent();
			MainPage = new MainWnd();
		}

		private Lazy<UnlockWnd> _unlockWnd = new Lazy<UnlockWnd>(() => new UnlockWnd(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
		private UnlockWnd UnlockWnd
		{
			get { return _unlockWnd.Value; }
		}

		async protected override void OnStart()
		{
			// TEST
			Debug.WriteLine("OnStart");
			var ttt = Settings.GetValueOrDefault("test", "[no data]");
			//await MainPage.DisplayAlert("settings...", ttt, "cancel");
			Settings.AddOrUpdateValue("test", "in OnStart");
			//

			// TODO: create credentials manager

			// TODO: pass credentials manager to data


			// TODO: if user want to: ask for MP/pin/biometrics
			//_unlock.UnlockMode();
			//MainPage.Navigation.PushModalAsync(_unlock, false);

			// TODO: prepare available FileSystems (with logins)

			// TODO: pass FileSystems to data


			// TODO: load data (minimum set -> global data settings, list of notebooks (minimum data))

			// TODO: restore last selections (with unlocking if necessary)

			//Notebook n = new Notebook()
			//{
			//	Name = "Notebook " + App.Data.Notebooks.Count
			//};
			//App.Data.Notebooks.Add(n);
			//App.Data.SelectNotebook(n);

			//Page p = new Page()
			//{
			//	Name = "Page _"
			//};
			//n.AddPage(p);
			//App.Data.SelectPage(p);

		}

		async protected override void OnSleep()
		{
			// TEST
			Debug.WriteLine("OnSleep");
			Settings.AddOrUpdateValue("test", "in OnSleep");
			//

			// TODO: if user want to: lock all data and clear all forms (unselect)
			//Data.SelectNotebook(null, false);
			//Data.SelectPage(null, false);

			// Show lock screen in order to hide data
			//_unlock.SplashMode();
			MainPage.Navigation.PushModalAsync(UnlockWnd, false);
			await Task.Delay(5000);
		}

		async protected override void OnResume()
		{
			// TEST
			Debug.WriteLine("OnResume");
			Settings.AddOrUpdateValue("test", "after OnResume");
			//

			// TODO: if user want to: ask for MP/pin/biometrics
			//_unlock.UnlockMode();
			// or
			// TODO: dispose lock screen
			await MainPage.Navigation.PopModalAsync();

			// TODO: if not was locked in OnSleep restore previously selected data
		}
	}
}
