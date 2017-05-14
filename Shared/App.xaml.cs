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

		static Lazy<Data> _Data = new Lazy<Data>(() => new Data(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
		public static Data Data
		{
			get { return _Data.Value; }
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

		private Lazy<UnlockWnd> _UnlockWnd = new Lazy<UnlockWnd>(() => new UnlockWnd(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
		private UnlockWnd UnlockWnd
		{
			get { return _UnlockWnd.Value; }
		}

		async protected override void OnStart()
		{
			Debug.WriteLine("OnStart");


			// TODO: create credentials manager
			// TODO: pass credentials manager to data


			UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnStart;

			// If user want to: ask for MP/pin/biometrics
			UnlockWnd.UnlockingMode();
			await MainPage.Navigation.PushModalAsync(UnlockWnd, false);
			// or
			// go on
			// UnlockedCorrectlyInOnStart(this, null);
		}

		async void UnlockedCorrectlyInOnStart(object sender, EventArgs e)
		{
			UnlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnStart;
			await Application.Current.MainPage.Navigation.PopModalAsync();

			// TODO: prepare available FileSystems (with logins)
			// TODO: pass FileSystems to data


			// TODO: load data (minimum set -> global data settings, list of notebooks (minimum data))
			// TODO: restore last selections (with unlocking if necessary)

			//await MainPage.DisplayAlert("settings...", ttt, "cancel");
			Notebook n = new Notebook()
			{
				Name = "Notebook " + App.Data.Notebooks.Count
			};
			App.Data.Notebooks.Add(n);
			App.Data.SelectNotebook(n);

			Page p = new Page()
			{
				Name = "Page _"
			};
			n.AddPage(p);
			App.Data.SelectPage(p);
			//await MainPage.DisplayAlert("settings... 2", ttt, "cancel");

		}


		async protected override void OnSleep()
		{
			Debug.WriteLine("OnSleep");

			// Do not do anything if unlocking is in progress (app loses focus because system needs to show some dialogs)
			if (UnlockWnd.State == UnlockWnd.TState.Unlocking)
				return;

			// TODO: if user want to: lock all data and clear all forms (unselect)
			//Data.SelectNotebook(null, false);
			//Data.SelectPage(null, false);

			// Show lock screen in order to hide data in task manager
			UnlockWnd.SplashMode();
			MainPage.Navigation.PushModalAsync(UnlockWnd, false);
			//await Task.Delay(5000);
		}


		async protected override void OnResume()
		{
			Debug.WriteLine("OnResume");

			// Do not do anything if not unlocked
			if (UnlockWnd.State != UnlockWnd.TState.Splash)
				return;

			UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnResume;

			// If user want to: ask for MP/pin/biometrics
			UnlockWnd.TryToUnlock();
			// or
			// dispose lock screen
			//UnlockedCorrectlyInOnResume(this, null);

		}

		async void UnlockedCorrectlyInOnResume(object sender, EventArgs e)
		{
			UnlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnResume;
			await Application.Current.MainPage.Navigation.PopModalAsync();

			// TODO: if not was locked in OnSleep restore previously selected data
		}

	}
}
