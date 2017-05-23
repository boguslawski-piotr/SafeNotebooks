using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
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

        static Lazy<ISecretsManager> _SecretsManager = new Lazy<ISecretsManager>(() => new SecretsManager(App.Name, new AesCryptographer(), new Settings.Storage()));
        public static ISecretsManager SecretsManager => _SecretsManager.Value;

        static Lazy<UnlockWnd> _UnlockWnd = new Lazy<UnlockWnd>(() => new UnlockWnd());
        static UnlockWnd UnlockWnd => _UnlockWnd.Value;

        //

        static Lazy<NotebooksManager> _DataManager = new Lazy<NotebooksManager>(() => new NotebooksManager(App.Name));
		public static NotebooksManager NotebooksManager => _DataManager.Value;

        static Lazy<NotebooksManagerUI> _DataManagerUI = new Lazy<NotebooksManagerUI>(() => new NotebooksManagerUI());
		public static NotebooksManagerUI DataManagerUI => _DataManagerUI.Value;

		//

		async void Tests()
        {
			//***
			
            string obs = "Ala ma kota";
            string obo = Obfuscator.Obfuscate(obs);
            string obd = Obfuscator.DeObfuscate(obo);

            //***

            //await SecretsManager.DeletePasswordAsync(App.Name);
            //await SecretsManager.AddOrUpdatePasswordAsync(App.Name, "1");
			//await SecretsManager.DeletePasswordAsync(App.Name);

			//***
			
            //DeviceFileSystem fs = new DeviceFileSystem(DeviceFileSystemRoot.Personal);
            //try
            //{
            //    string fn = "O-" + pbXNet.Tools.CreateGuid() + "-" + pbXNet.Tools.CreateGuid() + "-" + pbXNet.Tools.CreateGuid() + "-1";
            //    await fs.WriteTextAsync(fn, "1");
            //    string d = await fs.ReadTextAsync(fn);
            //    Debug.WriteLine(d);
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.ToString());
            //}

            //DeviceFileSystem fs = new DeviceFileSystem(DeviceFileSystemRoot.Personal);
			//await fs.SetCurrentDirectoryAsync("a");
			//IFileSystem fsc = await fs.MakeCopyAsync();
			//await fs.SetCurrentDirectoryAsync("..");

			//IEnumerable<string> d = await fs.GetDirectoriesAsync();
			//IEnumerable<string> f = await fs.GetFilesAsync();

			//bool de = await fs.DirectoryExistsAsync(".config");

			//await fs.WriteTextAsync("ala", "jakiś tekst");

			//bool fe = await fs.FileExistsAsync("ala");

			//try
			//{
			//    string tt = await fs.ReadTextAsync("ala");
			//}
			//catch { }

			//await fs.CreateDirectoryAsync("dir1");

			//***

			//var tests = new AesCryptographerTests();
			//tests.BasicEncryptDecrypt();
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

        async void ContinueOnStart()
        {
            Debug.WriteLine("ContinueOnStart");

            // Prepare available file systems
            // TODO: prepare available FileSystems other that device file system (with logins, etc.)

            foreach(DeviceFileSystemRoot root in  DeviceFileSystem.AvailableRootsForEndUser)
                NotebooksManager.AddFileSystem(new DeviceFileSystem(root));


			// Prepare DataManager

			NotebooksManager.SecretsManager = SecretsManager;
            NotebooksManager.UI = DataManagerUI;

            //

            await App.NotebooksManager.LoadNotebooksAsync(App.Settings.TryToUnlockItemChildren);

			// TODO: restore last selections (with unlocking if necessary)
			//await App.DataManager.SelectNotebookAsync(n);
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
                UnlockWnd.SetSplashMode();
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

        async void ContinueOnResume()
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
