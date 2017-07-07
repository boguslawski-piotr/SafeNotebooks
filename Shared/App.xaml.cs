using System;
using System.Reflection;
using System.Threading.Tasks;
using pbXNet;
using Xamarin.Forms;

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


		//

		void InitializeLocalization()
		{
			LocalizationManager.AddResource("SafeNotebooks.Texts.T", typeof(SafeNotebooks.Texts.Dummy).GetTypeInfo().Assembly);
		}

		void CreateSerializer()
		{
			Serializer = new NewtonsoftJsonSerializer();  // very, very fast
		}

		void CreateSafeStorage()
		{
			IPassword passwd = new Password(Obfuscator.Obfuscate(Tools.GetUaqpid()));
			Log.D($"pwd1: {passwd}", this);

			IFileSystem safeFs = new EncryptedFileSystem(App.Name, DeviceFileSystem.New(DeviceFileSystem.RootType.LocalConfig), passwd);

			SafeStorage = new StorageOnFileSystem<string>(App.Name, safeFs, Serializer);
		}

		void CreateSecretsManager()
		{
			CreateSerializer();
			CreateSafeStorage();

			IPassword passwd = new Password(Tools.GetUaqpid());
			Log.D($"pwd2: {passwd}", this);

			SecretsManager = new SecretsManager(App.Name, Serializer, SafeStorage, passwd);
		}

		void InitializeSecretsManager()
		{
		}

		void CreateStoragesManager()
		{
			StoragesManager = new StoragesManager(App.Name)
			{
				Serializer = this.Serializer
			};
		}

		async Task InitializeStoragesManagerAsync()
		{
			await SafeStorage.InitializeAsync();

			await StoragesManager.InitializeAsync();
		}

		void CreateNotebooksManager()
		{
			NotebooksManager = new NotebooksManager()
			{
				SecretsManager = this.SecretsManager,
				Serializer = this.Serializer,
			};
		}

		async Task InitializeNotebooksManagerAsync()
		{
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
			InitializeLocalization();

			CreateSecretsManager();

			CreateStoragesManager();

			CreateNotebooksManager();

			InitializeComponent();

			Tests();

			MainPage = new MainWnd();

			NotebooksManager.UI = MainWnd.Current;
		}

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

		//

		protected override async void OnStart()
		{
			Log.D("", this);

			_unlockWnd = new UnlockWnd();
			_unlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnStart;
			_unlockWnd.SetUnlockingMode();
			await MainPage.Navigation.PushModalAsync(_unlockWnd, false);
		}

		async void UnlockedCorrectlyInOnStart(object sender, EventArgs e)
		{
			Log.D("", this);

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
			Log.D("", this);

			try
			{
				InitializeSecretsManager();
				await InitializeStoragesManagerAsync();
				await InitializeNotebooksManagerAsync();
				await LoadNotebooksAsync();
			}
			catch (Exception ex)
			{
				Log.E(ex.Message, this);
				MainWnd.C.DisplayError(ex);
			}
		}


		//

		protected override async void OnSleep()
		{
			Log.D("", this);

			// Do not do anything if unlocking is in progress (app loses focus because system needs to show some dialogs)
			if (_unlockWnd != null)
			{
				if (_unlockWnd.State == UnlockWnd.TState.Unlocking)
				{
					Log.D("did nothing", this);
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
			Log.D("", this);

			// Do not do anything if not unlocked
			if (_unlockWnd == null || _unlockWnd.State != UnlockWnd.TState.Splash)
			{
				Log.D("did nothing", this);
				return;
			}

			_unlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnResume;
			_unlockWnd.TryToUnlockAsync();
		}

		async void UnlockedCorrectlyInOnResume(object sender, EventArgs e)
		{
			Log.D("", this);

			_unlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnResume;
			await Application.Current.MainPage.Navigation.PopModalAsync(true);
			_unlockWnd = null;

			ContinueOnResumeAsync();
		}

		async Task ContinueOnResumeAsync()
		{
			Log.D("", this);
			await Task.Delay(0);
		}

#pragma warning restore CS4014
	}
}
