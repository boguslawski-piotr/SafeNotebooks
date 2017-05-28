using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using pbXNet;
using pbXSecurity;
using Xamarin.Forms;

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SafeNotebooks
{
    public partial class App : Application
    {
        public static string Name
        {
            get {
                string name = typeof(App).GetTypeInfo().Assembly.ManifestModule.Name;
                int firstDot = name.IndexOf('.');
                if (firstDot > 0)
                    name = name.Substring(0, firstDot);
                return name;
            }
        }

        static Lazy<ISecretsManager> _SecretsManager = new Lazy<ISecretsManager>(() => new SecretsManager(App.Name, new AesCryptographer(), new Settings.Storage()));
        public static ISecretsManager SecretsManager => _SecretsManager.Value;

        static Lazy<UnlockWnd> _UnlockWnd = new Lazy<UnlockWnd>(() => new UnlockWnd());
        static UnlockWnd UnlockWnd => _UnlockWnd.Value;

        static Lazy<StoragesManager> _StoragesManager = new Lazy<StoragesManager>(() => new StoragesManager(App.Name));
        public static StoragesManager StoragesManager => _StoragesManager.Value;

        static Lazy<NotebooksManager> _NotebooksManager = new Lazy<NotebooksManager>(() => new NotebooksManager(App.Name));
        public static NotebooksManager NotebooksManager => _NotebooksManager.Value;

        static Lazy<NotebooksManagerUI> _NotebooksManagerUI = new Lazy<NotebooksManagerUI>(() => new NotebooksManagerUI());
        public static NotebooksManagerUI NotebooksManagerUI => _NotebooksManagerUI.Value;

		// Settings in AppSettings.cs


		//

		void InitializeLocalization()
        {
            LocalizationManager.AddResources("SafeNotebooks.Texts.T", typeof(SafeNotebooks.Texts.Dummy).GetTypeInfo().Assembly);
        }

        async Task InitializeSecrets()
        {
            // TODO: Potential security hole -> should be rethought more thoroughly
            //if (!await SecretsManager.PasswordExistsAsync(App.Name))
            //{
            //    Settings.UnlockUsingPin = false;
            //}
        }

        public App()
        {
            InitializeLocalization();
            InitializeSecrets();

#if DEBUG
            Tests();
#endif

            InitializeComponent();

            MainPage = new MainWnd();
        }


        //

        protected override void OnStart()
        {
            Debug.WriteLine("OnStart");

            if (UnlockWnd.UnlockingNeeded)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnStart;
                    UnlockWnd.SetUnlockingMode();
                    await MainPage.Navigation.PushModalAsync(UnlockWnd, false);
                });
            }
            else
            {
                ContinueOnStartAsync();
            }
        }

        async void UnlockedCorrectlyInOnStart(object sender, EventArgs e)
        {
            Debug.WriteLine("UnlockedCorrectlyInOnStart");

            UnlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnStart;

			// Give a little time for everything to be done in case there was 
            // no action on the UnlockWnd window displayed during OnStart execution.
			await Task.Delay(500); 

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.Navigation.PopModalAsync(false);
            });

            ContinueOnStartAsync();
        }

        async Task ContinueOnStartAsync()
        {
            Debug.WriteLine("ContinueOnStart");

            // Prepare available file systems/storages

            await StoragesManager.InitializeAsync();

            // Prepare Notebooks Manager

			NotebooksManager.SecretsManager = SecretsManager;
            NotebooksManager.UI = NotebooksManagerUI;
            await NotebooksManager.InitializeAsync();

			// Prepare background tasks
			

			// Load available notebooks

			await App.NotebooksManager.LoadNotebooksAsync(StoragesManager.Storages, App.Settings.TryToUnlockItemChildren);

			// TODO: restore last selections (with unlocking if necessary)
			//await App.DataManager.SelectNotebookAsync(n);
		}


        //

        protected override void OnSleep()
        {
            Debug.WriteLine("OnSleep");

            // Do not do anything if unlocking is in progress (app loses focus because system needs to show some dialogs)
            if (UnlockWnd.State == UnlockWnd.TState.Unlocking)
                return;
            IEnumerator<Xamarin.Forms.Page> ModalPages = MainPage.Navigation.ModalStack.GetEnumerator();
            ModalPages.Reset();
            while (ModalPages.MoveNext())
            {
                if (ModalPages.Current == UnlockWnd)
                    return;
            }


            // TODO: if user want to: lock all data and clear all forms (unselect)
            //Data.SelectNotebook(null, false);
            //Data.SelectPage(null, false);


            // Show lock screen in order to hide data in system task manager
            Device.BeginInvokeOnMainThread(async () =>
            {
                UnlockWnd.SetSplashMode();
                await MainPage.Navigation.PushModalAsync(UnlockWnd, false);
            });
            //await Task.Delay(5000);
        }


        //

        protected override void OnResume()
        {
            Debug.WriteLine("OnResume");

            // Do not do anything if not unlocked
            if (UnlockWnd.State != UnlockWnd.TState.Splash)
                return;

            if (UnlockWnd.UnlockingNeeded)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnResume;
                    UnlockWnd.TryToUnlock();
                });
            }
            else
            {
                ContinueOnResume();
            }

        }

        void UnlockedCorrectlyInOnResume(object sender, EventArgs e)
        {
            Debug.WriteLine("UnlockedCorrectlyInOnResume");

            UnlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnResume;

            ContinueOnResume();
        }

        void ContinueOnResume()
        {
            Debug.WriteLine("ContinueOnResume");

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.Navigation.PopModalAsync(false);
            });
        }

    }
}
