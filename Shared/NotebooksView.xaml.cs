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
    public partial class NotebooksView : ContentViewEx
    {
        public NotebooksView()
        {
            InitializeComponent();

            App.NotebooksManager.NotebooksLoadingBegun += NotebooksLoadingBegun;
            App.NotebooksManager.NotebooksLoaded += NotebooksLoaded;
            App.NotebooksManager.NotebooksSorted += NotebooksSorted;

            App.NotebooksManager.NotebookLoaded += NotebookLoaded;
            App.NotebooksManager.NotebookSelected += NotebookSelected;

            App.NotebooksManager.PagesSorted += PagesSorted;
            App.NotebooksManager.PageLoaded += PageLoaded;

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null; // disable item selection
            ListCtl.ItemTapped += ListCtl_ItemTapped;
        }

        protected override void ContinueOnSizeAllocated(double width, double height)
        {
        }


        //

        void NotebooksLoadingBegun(object sender, NotebooksManager man)
        {
        }

        void NotebooksLoaded(object sender, NotebooksManager man)
        {
        }

        void NotebooksSorted(object sender, NotebooksManager man)
        {
            Device.BeginInvokeOnMainThread(ShowSelectedNotebook);
        }

        void NotebookLoaded(object sender, Notebook notebook)
        {
        }

        void NotebookSelected(object sender, Notebook notebook)
        {
            Device.BeginInvokeOnMainThread(ShowSelectedNotebook);
        }

        void ShowSelectedNotebook()
        {
            BatchBegin();

            if (App.NotebooksManager.SelectedNotebook == null)
            {
                BackBtn.IsVisible = false;

                AppTitle.Margin = new Thickness(Metrics.ScreenEdgeMargin, 0, 0, 0);
                AppTitle.IsVisible = true;

                EditBtn.IsVisible = false;
                SettingsBtn.IsVisible = true;

                SelectedNotebookBar.IsVisible = false;

                ListCtl.BackgroundColor = (Color)App.Current.Resources["NotebooksListBackgroundColor"]; ;
                ToolBar.BackgroundColor = (Color)App.Current.Resources["NotebooksToolBarBackgroundColor"]; ;

                ViewsCommonLogic.ListViewSetItemsSource(ListCtl, App.NotebooksManager.ObservableItems);

                if (App.NotebooksManager.PreviouslySelectedNotebook != null)
                    ViewsCommonLogic.ListViewScrollTo(ListCtl, App.NotebooksManager.PreviouslySelectedNotebook);
            }
            else
            {
                BackBtn.IsVisible = true;

                AppTitle.IsVisible = false;
                AppTitle.Margin = new Thickness(0, 0, 0, 0);

                EditBtn.IsVisible = true;
                SettingsBtn.IsVisible = false;

                SelectedNotebookName.Text = App.NotebooksManager.SelectedNotebook.NameForLists;
                SelectedNotebookStorageName.Text = App.NotebooksManager.SelectedNotebook.Storage?.Name;
                SelectedNotebookBar.IsVisible = true;

                ListCtl.BackgroundColor = (Color)App.Current.Resources["PagesListBackgroundColor"];
                ToolBar.BackgroundColor = (Color)App.Current.Resources["PagesToolBarBackgroundColor"];

                ViewsCommonLogic.ListViewSetItemsSource(ListCtl, App.NotebooksManager.SelectedNotebook.ObservableItems);

                if (App.NotebooksManager.SelectedPage != null && App.NotebooksManager.SelectedPage.Notebook == App.NotebooksManager.SelectedNotebook)
                    ViewsCommonLogic.ListViewScrollTo(ListCtl, App.NotebooksManager.SelectedPage);
            }

            SearchBar.BackgroundColor = ListCtl.BackgroundColor;
            SearchQuery.BackgroundColor = ListCtl.BackgroundColor;

            BatchCommit();
        }


        //

        void PagesSorted(object sender, Notebook notebook)
        {
            Device.BeginInvokeOnMainThread(() => ViewsCommonLogic.ListViewSetItemsSource(ListCtl, App.NotebooksManager.SelectedNotebook?.ObservableItems));
        }

        void PageLoaded(object sender, Page page)
        {
        }


        //

        void ListCtl_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Notebook notebook)
            {
                if (App.NotebooksManager.SelectModeForItemsEnabled)
                    notebook.IsSelected = !notebook.IsSelected;
                else
                    App.NotebooksManager.SelectNotebookAsync(notebook, App.Settings.TryToUnlockItemChildren);
            }
            else
            {
                Page page = (Page)e.Item;
                if (page.Notebook.SelectModeForItemsEnabled)
                    page.IsSelected = !page.IsSelected;
                else
                    App.NotebooksManager.SelectPageAsync(page, App.Settings.TryToUnlockItemChildren);
            }
        }


        //

        void BackBtn_Clicked(object sender, System.EventArgs e)
        {
            App.NotebooksManager.SelectNotebookAsync(null, false);
        }

        async void SettingsBtn_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new SettingsWnd(), true);
        }

        void EditBtn_Clicked(object sender, System.EventArgs e)
        {
            App.NotebooksManager.SelectedNotebook?.EditAsync();
        }


        void SearchQuery_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            ViewsCommonLogic.SearchQuery_Focused(SearchBar, SearchQuery, CancelSearchBtn);
        }

        void SearchQuery_Unfocused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            ViewsCommonLogic.SearchQuery_Unfocused(SearchBar, SearchQuery, CancelSearchBtn);
        }

        void SearchQuery_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void CancelSearchBtn_Clicked(object sender, System.EventArgs e)
        {
            ViewsCommonLogic.CancelSearchBtn_Clicked(SearchBar, SearchQuery, CancelSearchBtn);
        }


        async void SortBtn_Clicked(object sender, System.EventArgs e)
        {
            //string[] cs = { "#ffff0000", "#ff008000", "#80ff0000", "#80008000", "#40ff0000", "#40008000", };
            //foreach (var c in cs)
            //{
            //    Notebook n = await App.NotebooksManager.NewNotebookAsync(App.StoragesManager.Storages.First());
            //    n.Color = Color.FromHex(c);
            //    await n.SaveAsync();
            //}
            //App.NotebooksManager.SortNotebooks();

            ItemWithItems.SortParameters sortParams = null;
            string title;

            if (App.NotebooksManager.SelectedNotebook == null)
            {
                sortParams = App.NotebooksManager.SortParameters;
                title = "Jak chcesz sortować notatniki?"; // TODO: localization
            }
            else
            {
                sortParams = App.NotebooksManager.SelectedNotebook.SortParameters;
                title = "Jak chcesz sortować strony?"; // TODO: localization
            }

            ChooseSortDlg d = new ChooseSortDlg(title, sortParams);

            d.Cancel += async (s, cp) => await App.MainWnd.NavigationEx.PopModalAsync();
            d.OK += async (object s, EventArgs op) =>
            {
                await App.MainWnd.NavigationEx.PopModalAsync();
                App.NotebooksManager.SortItems();
            };

            await App.MainWnd.NavigationEx.PushModalAsync(d, DeviceEx.Orientation == DeviceOrientation.Landscape ? NavigationEx.ModalPosition.BottomLeft : NavigationEx.ModalPosition.BottomCenter);
		}


        async Task<StorageOnFileSystem<string>> SelectStorageUIAsync(IEnumerable<StorageOnFileSystem<string>> storages)
        {
            string fsName = await App.Current.MainPage.DisplayActionSheet("Where do you want to store the notebook?", // TODO: localization
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
            if (App.NotebooksManager.SelectedNotebook == null)
            {
                StorageOnFileSystem<string> storage = await App.StoragesManager.SelectStorageAsync(SelectStorageUIAsync);
                if (storage != null)
                {
                    Notebook n = await App.NotebooksManager.NewNotebookAsync(storage);
                    if (n != null)
                        App.NotebooksManager.SelectNotebookAsync(n, App.Settings.TryToUnlockItemChildren);
                }
            }
            else
            {
                Page p = await App.NotebooksManager.SelectedNotebook.NewPageAsync();
                if (p != null)
                {
                    await App.NotebooksManager.SelectPageAsync(p, App.Settings.TryToUnlockItemChildren);
                    Device.BeginInvokeOnMainThread(() => ViewsCommonLogic.ListViewScrollTo(ListCtl, p));
                }
            }
        }


        void EditItemsBtn_Clicked(object sender, System.EventArgs e)
        {
            //Application.Current.MainPage.DisplayAlert("Edit items...", "edit multiple items", "Cancel");
            if (App.NotebooksManager.SelectedNotebook == null)
                App.NotebooksManager.SelectModeForItemsEnabled = !App.NotebooksManager.SelectModeForItemsEnabled;
            else
                App.NotebooksManager.SelectedNotebook.SelectModeForItemsEnabled = !App.NotebooksManager.SelectedNotebook.SelectModeForItemsEnabled;
        }

    }
}
