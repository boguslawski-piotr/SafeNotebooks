using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;
using pbXSecurity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#if __ANDROID__
using SafeNotebooks.Droid;
#endif

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SafeNotebooks
{
	public partial class App : Application
	{
		public static string Name
		{
			get {
				try
				{
					string name = typeof(App).GetTypeInfo().Assembly.ManifestModule.Name;
					int firstDot = name.IndexOf('.');
					if (firstDot > 0)
						name = name.Substring(0, firstDot);
					return name;
				}
				catch
				{
					return "Safe Notebooks";
				}
			}
		}


		ISecretsManager _SecretsManager;
		public static ISecretsManager SecretsManager => (Application.Current as App)._SecretsManager;

		StoragesManager _StoragesManager = new StoragesManager(App.Name);
		public static StoragesManager StoragesManager => (Application.Current as App)._StoragesManager;

		NotebooksManager _NotebooksManager = new NotebooksManager();
		public static NotebooksManager NotebooksManager => (Application.Current as App)._NotebooksManager;

		// Settings in AppSettings.cs


		//

		UnlockWnd UnlockWnd;

		static DateTime _startTime = DateTime.Now;
		TimeSpan _timeFromStart => DateTime.Now - _startTime;

		void InitializeLocalization()
		{
			LocalizationManager.AddResource("SafeNotebooks.Texts.T", typeof(SafeNotebooks.Texts.Dummy).GetTypeInfo().Assembly, true);
			LocalizationManager.AddResource("pbXNet.Texts.T", typeof(pbXNet.LocalizationManager).GetTypeInfo().Assembly);
		}

		void InitializeSecretsManager()
		{
			if (_SecretsManager != null)
				return;

			_SecretsManager = new SecretsManager(App.Name, new AesCryptographer(), Settings.Current.Storage);

#if __ANDROID__
			// TODO: jakos lepiej to rozwiazac?
			_SecretsManager.Initialize(MainActivity.Current);
#endif

			// TODO: do wywalenia!
			_SecretsManager.AddOrUpdatePasswordAsync(App.Name, "1");
			// ***
		}

		public App()
		{
			Debug.WriteLine($"App constructor: {_timeFromStart}");

			InitializeLocalization();
			InitializeSecretsManager();
			InitializeComponent();

#if DEBUG
			Tests();
#endif
		}

		protected override async void OnStart()
		{
			Debug.WriteLine($"OnStart: {_timeFromStart}");

			MainPage = new MainWnd();

			UnlockWnd = new UnlockWnd();
			UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnStart;
			UnlockWnd.SetUnlockingMode();
			await MainPage.Navigation.PushModalAsync(UnlockWnd, false);
		}

		async void UnlockedCorrectlyInOnStart(object sender, EventArgs e)
		{
			Debug.WriteLine($"UnlockedCorrectlyInOnStart: {_timeFromStart}");

			UnlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnStart;

			// Give a little time for everything to be done in case there was 
			// no action on the UnlockWnd window displayed during OnStart execution.
			await Task.Delay(500);

			await Application.Current.MainPage.Navigation.PopModalAsync(true);
			UnlockWnd = null;

			ContinueOnStartAsync();
		}

		async Task ContinueOnStartAsync()
		{
			Debug.WriteLine($"ContinueOnStart: {_timeFromStart}");

			// Prepare available file systems/storages

			if (SecretsManager == null)
				InitializeSecretsManager();

			await _StoragesManager.InitializeAsync();

			// Prepare Notebooks Manager

			NotebooksManager.SecretsManager = SecretsManager;
			NotebooksManager.UI = new NotebooksManagerUI();
			await NotebooksManager.InitializeAsync(Settings.Current.Storage);

			// Load/reload available notebooks

			await App.NotebooksManager.LoadNotebooksAsync(StoragesManager.Storages, App.Settings.TryToUnlockItemItems);

			// TODO: restore last selections (with unlocking if necessary)
			//await App.DataManager.SelectNotebookAsync(n);

			Debug.WriteLine($"ContinueOnStart: END: {_timeFromStart}");
		}


		//

		protected override async void OnSleep()
		{
			Debug.WriteLine("OnSleep");

			// Do not do anything if unlocking is in progress (app loses focus because system needs to show some dialogs)
			if (UnlockWnd != null)
			{
				if (UnlockWnd.State == UnlockWnd.TState.Unlocking)
				{
					Debug.WriteLine("OnSleep: did nothing");
					return;
				}
			}

			// TODO: if user want to: lock all data and clear all forms (unselect)
			//Data.SelectNotebook(null, false);
			//Data.SelectPage(null, false);

			// Show lock screen in order to hide data in system task manager
			UnlockWnd = new UnlockWnd();
			UnlockWnd.SetSplashMode();
			await MainPage.Navigation.PushModalAsync(UnlockWnd, false);
		}


		//

		protected override void OnResume()
		{
			Debug.WriteLine("OnResume");

			// Do not do anything if not unlocked
			if (UnlockWnd == null || UnlockWnd.State != UnlockWnd.TState.Splash)
			{
				Debug.WriteLine("OnResume: did nothing");
				return;
			}

			UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnResume;
			UnlockWnd.TryToUnlock();
		}

		async void UnlockedCorrectlyInOnResume(object sender, EventArgs e)
		{
			Debug.WriteLine("UnlockedCorrectlyInOnResume");

			UnlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnResume;
			await Application.Current.MainPage.Navigation.PopModalAsync(true);
			UnlockWnd = null;

			ContinueOnResumeAsync();
		}

		async Task ContinueOnResumeAsync()
		{
			Debug.WriteLine("ContinueOnResume");
		}
	}
}
