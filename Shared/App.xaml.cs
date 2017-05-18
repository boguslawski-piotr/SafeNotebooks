using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using pbXForms;
using pbXSecurity;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;

using System.Reflection;
using pbXNet;

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

        public static class Settings
        {
            public static ISettings Current
            {
                get { return CrossSettings.Current; }
            }

            // Security

            private const string UnlockUsingSystemKey = "_uus";
            static readonly bool UnlockUsingSystemDefault = false;
            private const string UnlockUsingPinKey = "_uup";
            static readonly bool UnlockUsingPinDefault = false;

            public static bool UnlockUsingSystem
            {
                get {
                    return Current.GetValueOrDefault<bool>(UnlockUsingSystemKey, UnlockUsingSystemDefault);
                }
                set {
                    Current.AddOrUpdateValue<bool>(UnlockUsingSystemKey, value);
                }
            }

            public static bool UnlockUsingPin
            {
                get {
                    return Current.GetValueOrDefault<bool>(UnlockUsingPinKey, UnlockUsingPinDefault);
                }
                set {
                    Current.AddOrUpdateValue<bool>(UnlockUsingPinKey, value);
                }
            }
        }

        //

        static Lazy<SecretsManager> _SecretsManager = new Lazy<SecretsManager>(() => new SecretsManager(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static SecretsManager SecretsManager
        {
            get { return _SecretsManager.Value; }
        }

        private Lazy<UnlockWnd> _UnlockWnd = new Lazy<UnlockWnd>(() => new UnlockWnd(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        private UnlockWnd UnlockWnd
        {
            get { return _UnlockWnd.Value; }
        }

        //

        async void Tests()
        {
			DeviceFileSystem fs = new DeviceFileSystem();
			await fs.SetCurrentDirectoryAsync("a");
            IFileSystem fsc = await fs.MakeCopyAsync();
			await fs.SetCurrentDirectoryAsync("..");

            IEnumerable<string> d = await fs.GetDirectoriesAsync();
			IEnumerable<string> f = await fs.GetFilesAsync();

            bool de = await fs.DirectoryExistsAsync(".config");

            await fs.WriteTextAsync("ala", "jakiś tekst");

            bool fe = await fs.FileExistsAsync("ala");

            string tt = await fs.ReadTextAsync("ala");

            await fs.CreateDirectoryAsync("dir1");

			//var tests = new CryptographerTests();
			//tests.BasicEncryptDecrypt();
		}

        public App()
        {
            LocalizationManager.AddResources("SafeNotebooks.Texts.T", typeof(SafeNotebooks.Texts.Texts).GetTypeInfo().Assembly);

            Tests();

            InitializeComponent();
            MainPage = new MainWnd();
        }

        protected override void OnStart()
        {
            Debug.WriteLine("OnStart");


            // TODO: pass credentials manager to data


            if (App.Settings.UnlockUsingSystem || App.Settings.UnlockUsingPin)
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

            if (App.Settings.UnlockUsingSystem || App.Settings.UnlockUsingPin)
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
