using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class NotebooksView : BaseView
    {
        public NotebooksView()
        {
            InitializeComponent();

            MainWnd.Current.MasterViewWillBeShown += MasterViewWillBeShown;

            App.NotebooksManager.NotebooksAreStartingToLoad += NotebooksAreStartingToLoad;
            App.NotebooksManager.NotebooksLoaded += NotebooksLoaded;

            App.NotebooksManager.ItemObservableItemsCreated += ObservableItemsCreated;

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null; // disable item selection
            ListCtl.ItemTapped += ListCtl_ItemTapped;

            ListCtl.IsPullToRefreshEnabled = true;
            ListCtl.RefreshCommand = new Command(RefreshNotebooks);

            InitializeSearchBarFor(ListCtl);
        }

        protected override void ContinueOnSizeAllocated(double width, double height)
        {
            base.ContinueOnSizeAllocated(width, height);
            if (MainWnd.Current.IsSplitView)
            {
                AppTitle.FontSize = Device.GetNamedSize(NamedSize.Medium, AppTitle);
            }
            else
                AppTitle.FontSize = Device.GetNamedSize(NamedSize.Large, AppTitle);
        }

        //

        void MasterViewWillBeShown(object sender, (View view, object param) e)
        {
            if (e.view == this)
                Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollTo(ListCtl, App.NotebooksManager.SelectedNotebook));
        }

        void NotebooksAreStartingToLoad(object sender, EventArgs ea)
        {
            Device.BeginInvokeOnMainThread(() => AIIsVisible(true));
        }

        void ObservableItemsCreated(object sender, ItemWithItems forWhom)
        {
            if (forWhom is NotebooksManager)
                Device.BeginInvokeOnMainThread(() => ListCtl.ItemsSource = forWhom.ObservableItems);
        }

        void NotebooksLoaded(object sender, bool anyNotebookLoaded)
        {
            Device.BeginInvokeOnMainThread(() => AIIsVisible(false));
        }

        void AIIsVisible(bool isVisible)
        {
            AI.IsVisible = isVisible;
            AI.IsRunning = isVisible;
        }


        //

        async void RefreshNotebooks()
        {
            await App.NotebooksManager.LoadNotebooksAsync(App.StoragesManager.Storages, App.Settings.TryToUnlockItemItems);
            Device.BeginInvokeOnMainThread(ListCtl.EndRefresh);
        }

        async Task SelectNotebook(Notebook notebook)
        {
            await MainWnd.Current.ShowMasterViewAsync<NotebookView>(MasterDetailPageEx.ViewsSwitchingAnimation.Forward, notebook);
            await App.NotebooksManager.SelectNotebookAsync(notebook, App.Settings.TryToUnlockItemItems);
        }

        async void ListCtl_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Notebook notebook)
            {
                if (App.NotebooksManager.SelectModeForItemsEnabled)
                    notebook.IsSelected = !notebook.IsSelected;
                else
                {
                    await SelectNotebook(notebook);
                }
            }
        }


        //

        async void SettingsBtn_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new SettingsWnd(), true);
        }


        //

        public override void SearchQueryChanged(string text)
        {
        }


        //

        void SortBtn_Clicked(object sender, System.EventArgs e)
        {
            //string[] cs = { "#ffff0000", "#ff008000", "#80ff0000", "#80008000", "#40ff0000", "#40008000", };
            //foreach (var c in cs)
            //{
            //    Notebook n = await App.NotebooksManager.NewNotebookAsync(App.StoragesManager.Storages.First());
            //    n.Color = Color.FromHex(c);
            //    //await n.SaveAsync();
            //}
            //App.NotebooksManager.SortNotebooks();

            base.SortBtn_Clicked(T.Localized("Notebooks"), App.NotebooksManager, ListCtl);
        }

        async Task<StorageOnFileSystem<string>> SelectStorageUIAsync(IEnumerable<StorageOnFileSystem<string>> storages)
        {
            // TODO: do zmiany na wlasny dialog (modal view)
            string fsName = await App.Current.MainPage.DisplayActionSheet(T.Localized("WhereStoreNotebook"),
                                                                          T.Localized("Cancel"),
                                                                          null,
                                                                          storages.Select((storage1) => storage1.Name).ToArray());
            try
            {
                return storages.First((storage2) => storage2.Name == fsName);
            }
            catch
            {
                return null;
            }
        }

        async void NewBtn_Clicked(object sender, System.EventArgs e)
        {
            StorageOnFileSystem<string> storage = await App.StoragesManager.SelectStorageAsync(SelectStorageUIAsync);
            if (storage != null)
            {
                //for (int i = 0; i < 200; i++)
                //{
                //    await App.NotebooksManager.NewNotebookAsync(storage);
                //}
                Notebook notebook = await App.NotebooksManager.NewNotebookAsync(storage);
                if (notebook != null)
                {
                    Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollTo(ListCtl, notebook));
                    await SelectNotebook(notebook);
                }
            }
        }

        void EditItemsBtn_Clicked(object sender, System.EventArgs e)
        {
            App.NotebooksManager.SelectModeForItemsEnabled = !App.NotebooksManager.SelectModeForItemsEnabled;
        }
    }
}
