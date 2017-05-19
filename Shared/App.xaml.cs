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
        public static readonly string Name = typeof(App).GetTypeInfo().Assembly.ManifestModule.Name.Replace(".exe", "");

        // Settings in AppSettings.cs

        //

        static Lazy<Data> _Data = new Lazy<Data>(() => new Data());
        public static Data Data
        {
            get { return _Data.Value; }
        }

        //

        static Lazy<ISecretsManager> _SecretsManager = new Lazy<ISecretsManager>(() => new SecretsManager(App.Name, new AesCryptographer(), new Settings.Storage()));
        public static ISecretsManager SecretsManager
        {
            get { return _SecretsManager.Value; }
        }

        private Lazy<UnlockWnd> _UnlockWnd = new Lazy<UnlockWnd>(() => new UnlockWnd());
        private UnlockWnd UnlockWnd
        {
            get { return _UnlockWnd.Value; }
        }

        //

        async void Tests()
        {
			await SecretsManager.AddOrUpdatePasswordAsync(App.Name, "1");
            //await SecretsManager.DeletePasswordAsync(App.Name);

			DeviceFileSystem fs = new DeviceFileSystem();
			await fs.SetCurrentDirectoryAsync("a");
            IFileSystem fsc = await fs.MakeCopyAsync();
			await fs.SetCurrentDirectoryAsync("..");

            IEnumerable<string> d = await fs.GetDirectoriesAsync();
			IEnumerable<string> f = await fs.GetFilesAsync();

            bool de = await fs.DirectoryExistsAsync(".config");

            await fs.WriteTextAsync("ala", "jakiś tekst");

            bool fe = await fs.FileExistsAsync("ala");

            try
            {
                string tt = await fs.ReadTextAsync("ala");
            }
            catch { }

            await fs.CreateDirectoryAsync("dir1");

			var tests = new AesCryptographerTests();
			tests.BasicEncryptDecrypt();
		}

        void InitializeLocalization()
        {
            LocalizationManager.AddResources("SafeNotebooks.Texts.T", typeof(SafeNotebooks.Texts.Texts).GetTypeInfo().Assembly);
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

            Tests();

            InitializeComponent();
            MainPage = new MainWnd();
        }

        protected override void OnStart()
        {
            Debug.WriteLine("OnStart");

            Data.SecretsManager = SecretsManager;

            if (App.Settings.UnlockUsingDeviceOwnerAuthentication || App.Settings.UnlockUsingPin)
            {
                UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnStart;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    UnlockWnd.UnlockingMode();
                    await MainPage.Navigation.PushModalAsync(UnlockWnd, false);
                });
            }
            else
            {
                ContinueOnStart();
            }
        }

        async void UnlockedCorrectlyInOnStart(object sender, EventArgs e)
        {
            Debug.WriteLine("UnlockedCorrectlyInOnStart");

            UnlockWnd.UnlockedCorrectly -= UnlockedCorrectlyInOnStart;

            await Task.Delay(500); // give a little time for everything to be done in case there was no action on the UnlockWnd window displayed during OnStart execution
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.Navigation.PopModalAsync(false);
            });

            ContinueOnStart();
        }

        void ContinueOnStart()
        {
            Debug.WriteLine("ContinueOnStart");

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
            //App.Data.SelectPage(p);
            App.Data.SelectPage(null);
            //await MainPage.DisplayAlert("settings... 2", ttt, "cancel");
        }


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


            // Show lock screen in order to hide data in task manager
            Device.BeginInvokeOnMainThread(async () =>
            {
                UnlockWnd.SplashMode();
                await MainPage.Navigation.PushModalAsync(UnlockWnd, false);
            });
            //await Task.Delay(5000);
        }


        protected override void OnResume()
        {
            Debug.WriteLine("OnResume");

            // Do not do anything if not unlocked
            if (UnlockWnd.State != UnlockWnd.TState.Splash)
                return;

            if (App.Settings.UnlockUsingDeviceOwnerAuthentication || App.Settings.UnlockUsingPin)
            {
                UnlockWnd.UnlockedCorrectly += UnlockedCorrectlyInOnResume;

                Device.BeginInvokeOnMainThread(() =>
                {
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

            // TODO: if data was not locked in OnSleep restore previously selected data
        }

    }
}
