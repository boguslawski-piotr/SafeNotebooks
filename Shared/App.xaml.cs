﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

#if __ANDROID__
using SafeNotebooks.Droid;
#endif

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SafeNotebooks
{
	public partial class App : Application
	{
		static string _name;
		public static string Name
		{
			get {
				try
				{
					if (_name == null)
					{
						string name = typeof(App).GetTypeInfo().Assembly.ManifestModule.Name;
						int firstDot = name.IndexOf('.');
						if (firstDot > 0)
							name = name.Substring(0, firstDot);
						_name = name;
					}
					return _name;
				}
				catch
				{
					return "Safe Notebooks";
				}
			}
		}

		public static App C => (Current as App);

		// Settings in AppSettings.cs

		public ISerializer Serializer;

		public ISearchableStorage<string> SafeStorage;

		public ISecretsManager SecretsManager;

		public StoragesManager StoragesManager;

		public NotebooksManager NotebooksManager;


		//

		UnlockWnd _unlockWnd;

		static DateTime _startTime = DateTime.Now;
		TimeSpan _timeFromStart => DateTime.Now - _startTime;


		//

		void InitializeLocalization()
		{
			LocalizationManager.AddResource("SafeNotebooks.Texts.T", typeof(SafeNotebooks.Texts.Dummy).GetTypeInfo().Assembly, true);
			LocalizationManager.AddResource("pbXNet.Texts.T", typeof(pbXNet.LocalizationManager).GetTypeInfo().Assembly);
		}

		void CreateSerializer()
		{
			Serializer = new NewtonsoftJsonSerializer();  // very, very fast
			//Serializer = new BinarySerializer();          // slow and produce 2x larger files
		}

		void CreateSafeStorage()
		{
			byte[] pwd = Obfuscator.Obfuscate(Tools.GetUaqpid()).ToByteArray();
			Debug.WriteLine($"CreateSafeStorage: pwd: {pwd.ToHexString()}");

			IFileSystem SafeFs = new EncryptedFileSystem(App.Name, new DeviceFileSystem(DeviceFileSystemRoot.Config), new AesCryptographer(), EncryptedFileSystem.CKeyType.Password, pwd);
			SafeStorage = new StorageOnFileSystem<string>(App.Name, SafeFs, Serializer);
		}

		void CreateSecretsManager()
		{
			CreateSerializer();
			CreateSafeStorage();
			SecretsManager = new SecretsManager(App.Name, new AesCryptographer(), SafeStorage, Serializer);
		}

		void InitializeSecretsManager()
		{
#if __ANDROID__
			// TODO: jakos lepiej to rozwiazac?
			SecretsManager.Initialize(MainActivity.Current);
#endif
		}

		void CreateStoragesManager()
		{
			StoragesManager = new StoragesManager(App.Name);
		}

		async Task InitializeStoragesManagerAsync()
		{
			await SafeStorage.InitializeAsync();

			StoragesManager.Serializer = Serializer;
			await StoragesManager.InitializeAsync();
		}

		void CreateNotebooksManager()
		{
			NotebooksManager = new NotebooksManager();
		}

		async Task InitializeNotebooksManagerAsync()
		{
			NotebooksManager.Serializer = Serializer;
			NotebooksManager.SecretsManager = SecretsManager;
			NotebooksManager.UI = new NotebooksManagerUI();
			await NotebooksManager.InitializeAsync(SafeStorage);
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

			CreateSecretsManager();

			InitializeSecretsManager();

			CreateStoragesManager();

			CreateNotebooksManager();

			InitializeComponent();

			Tests();

			MainPage = new MainWnd();
		}


		//

		protected override async void OnStart()
		{
			Debug.WriteLine($"OnStart: {_timeFromStart}");

			_unlockWnd = new UnlockWnd();
			_unlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnStart;
			_unlockWnd.SetUnlockingMode();
			await MainPage.Navigation.PushModalAsync(_unlockWnd, false);
		}

		async void UnlockedCorrectlyInOnStart(object sender, EventArgs e)
		{
			Debug.WriteLine($"UnlockedCorrectlyInOnStart: {_timeFromStart}");

			// Give a little time for everything to be done in case there was 
			// no action on the UnlockWnd window displayed during OnStart execution.
			await Task.Delay(500);

			_unlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnStart;
			await Application.Current.MainPage.Navigation.PopModalAsync(true);
			_unlockWnd = null;

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
			if (_unlockWnd != null)
			{
				if (_unlockWnd.State == UnlockWnd.TState.Unlocking)
				{
					Debug.WriteLine("OnSleep: did nothing");
					return;
				}
			}

			// TODO: if user want to: lock all data and clear all forms (unselect)
			//Data.SelectNotebook(null, false);
			//Data.SelectPage(null, false);

			// Show lock screen in order to hide data in system task manager
			_unlockWnd = new UnlockWnd();
			_unlockWnd.SetSplashMode();
			await MainPage.Navigation.PushModalAsync(_unlockWnd, false);
		}


		//

		protected override void OnResume()
		{
			Debug.WriteLine("OnResume");

			// Do not do anything if not unlocked
			if (_unlockWnd == null || _unlockWnd.State != UnlockWnd.TState.Splash)
			{
				Debug.WriteLine("OnResume: did nothing");
				return;
			}

			_unlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnResume;
			_unlockWnd.TryToUnlockAsync();
		}

		async void UnlockedCorrectlyInOnResume(object sender, EventArgs e)
		{
			Debug.WriteLine("UnlockedCorrectlyInOnResume");

			_unlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnResume;
			await Application.Current.MainPage.Navigation.PopModalAsync(true);
			_unlockWnd = null;

			ContinueOnResumeAsync();
		}

		async Task ContinueOnResumeAsync()
		{
			Debug.WriteLine("ContinueOnResume");
		}
	}
}
