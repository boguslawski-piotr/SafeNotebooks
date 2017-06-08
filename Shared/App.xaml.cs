using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;
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
		static string _Name;
		public static string Name
		{
			get {
				try
				{
					if (_Name == null)
					{
						string name = typeof(App).GetTypeInfo().Assembly.ManifestModule.Name;
						int firstDot = name.IndexOf('.');
						if (firstDot > 0)
							name = name.Substring(0, firstDot);
						_Name = name;
					}
					return _Name;
				}
				catch
				{
					return "Safe Notebooks";
				}
			}
		}

		Lazy<ISerializer> _Serializer = new Lazy<ISerializer>(() => new NewtonsoftJsonSerializer());
		public static ISerializer Serializer => (Application.Current as App)._Serializer.Value;

		Lazy<ISecretsManager> _SecretsManager = new Lazy<ISecretsManager>(() => new SecretsManager(App.Name, new AesCryptographer(), Settings.Storage, Serializer));
		public static ISecretsManager SecretsManager => (Application.Current as App)._SecretsManager.Value;

		Lazy<StoragesManager> _StoragesManager = new Lazy<StoragesManager>(() => new StoragesManager(App.Name));
		public static StoragesManager StoragesManager => (Application.Current as App)._StoragesManager.Value;

		Lazy<NotebooksManager> _NotebooksManager = new Lazy<NotebooksManager>(() => new NotebooksManager());
		public static NotebooksManager NotebooksManager => (Application.Current as App)._NotebooksManager.Value;

		// Settings in AppSettings.cs


		//

		UnlockWnd _UnlockWnd;

		static DateTime _startTime = DateTime.Now;
		TimeSpan _timeFromStart => DateTime.Now - _startTime;


		//

		void InitializeLocalization()
		{
			LocalizationManager.AddResource("SafeNotebooks.Texts.T", typeof(SafeNotebooks.Texts.Dummy).GetTypeInfo().Assembly, true);
			LocalizationManager.AddResource("pbXNet.Texts.T", typeof(pbXNet.LocalizationManager).GetTypeInfo().Assembly);
		}

		void InitializeSecretsManager()
		{
#if __ANDROID__
			// TODO: jakos lepiej to rozwiazac?
			_SecretsManager.Initialize(MainActivity.Current);
#endif
		}

		async Task InitializeStoragesManagerAsync()
		{
			StoragesManager.Serializer = Serializer;
			await StoragesManager.InitializeAsync();
		}

		async Task InitializeNotebooksManagerAsync()
		{
			NotebooksManager.Serializer = Serializer;
			NotebooksManager.SecretsManager = SecretsManager;
			NotebooksManager.UI = new NotebooksManagerUI();
			await NotebooksManager.InitializeAsync(Settings.Storage);
		}

		async Task LoadNotebooksAsync()
		{
			await NotebooksManager.LoadNotebooksAsync(StoragesManager.Storages, Settings.TryToUnlockItemItems);

			// TODO: restore last selections (with unlocking if necessary)
			//await App.DataManager.SelectNotebookAsync(n);
		}


		//

		public App()
		{
			Debug.WriteLine($"App constructor: {_timeFromStart}");

			InitializeLocalization();
			InitializeSecretsManager();
			InitializeComponent();

			MainPage = new MainWnd();
		}


		//

		protected override async void OnStart()
		{
			Debug.WriteLine($"OnStart: {_timeFromStart}");

			_UnlockWnd = new UnlockWnd();
			_UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnStart;
			_UnlockWnd.SetUnlockingMode();
			await MainPage.Navigation.PushModalAsync(_UnlockWnd, false);
		}

		async void UnlockedCorrectlyInOnStart(object sender, EventArgs e)
		{
			Debug.WriteLine($"UnlockedCorrectlyInOnStart: {_timeFromStart}");

			// Give a little time for everything to be done in case there was 
			// no action on the UnlockWnd window displayed during OnStart execution.
			await Task.Delay(500);

			_UnlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnStart;
			await Application.Current.MainPage.Navigation.PopModalAsync(true);
			_UnlockWnd = null;

			ContinueOnStartAsync();
		}

		async Task ContinueOnStartAsync()
		{
			Debug.WriteLine($"ContinueOnStart: {_timeFromStart}");

			InitializeSecretsManager();
			await InitializeStoragesManagerAsync();
			await InitializeNotebooksManagerAsync();
			await LoadNotebooksAsync();

			Debug.WriteLine($"ContinueOnStart: END: {_timeFromStart}");
		}


		//

		protected override async void OnSleep()
		{
			Debug.WriteLine("OnSleep");

			// Do not do anything if unlocking is in progress (app loses focus because system needs to show some dialogs)
			if (_UnlockWnd != null)
			{
				if (_UnlockWnd.State == UnlockWnd.TState.Unlocking)
				{
					Debug.WriteLine("OnSleep: did nothing");
					return;
				}
			}

			// TODO: if user want to: lock all data and clear all forms (unselect)
			//Data.SelectNotebook(null, false);
			//Data.SelectPage(null, false);

			// Show lock screen in order to hide data in system task manager
			_UnlockWnd = new UnlockWnd();
			_UnlockWnd.SetSplashMode();
			await MainPage.Navigation.PushModalAsync(_UnlockWnd, false);
		}


		//

		protected override void OnResume()
		{
			Debug.WriteLine("OnResume");

			// Do not do anything if not unlocked
			if (_UnlockWnd == null || _UnlockWnd.State != UnlockWnd.TState.Splash)
			{
				Debug.WriteLine("OnResume: did nothing");
				return;
			}

			_UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnResume;
			_UnlockWnd.TryToUnlock();
		}

		async void UnlockedCorrectlyInOnResume(object sender, EventArgs e)
		{
			Debug.WriteLine("UnlockedCorrectlyInOnResume");

			_UnlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnResume;
			await Application.Current.MainPage.Navigation.PopModalAsync(true);
			_UnlockWnd = null;

			ContinueOnResumeAsync();
		}

		async Task ContinueOnResumeAsync()
		{
			Debug.WriteLine("ContinueOnResume");
		}
	}
}
