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

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null;
            ListCtl.ItemTapped += (object sender, ItemTappedEventArgs e) =>
            {
                if (e.Item is Notebook)
                    App.NotebooksManager.SelectNotebookAsync((Notebook)e.Item, App.Settings.TryToUnlockItemChildren);
                else
                    App.NotebooksManager.SelectPageAsync((Page)e.Item, App.Settings.TryToUnlockItemChildren);
            };
        }

        protected override void ContinueOnSizeAllocated(double width, double height)
        {
        }

        void ListCtlScrollTo(Item item)
        {
            ListCtl.ScrollTo(item, ScrollToPosition.Center, false);
        }

        void ListCtlSetItemsSource(IEnumerable l)
        {
            ListCtl.BeginRefresh();

            IEnumerator e = l.GetEnumerator();
            e.Reset();
            object o = null;
            if (e.MoveNext())
                o = e.Current;

            ListCtl.ItemsSource = l;

            if (o != null)
                ListCtl.ScrollTo(o, ScrollToPosition.Start, false);

            ListCtl.EndRefresh();
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

                AppTitle.IsVisible = true;
                AppTitle.Margin = new Thickness(Metrics.ScreenEdgeMargin, 0, 0, 0);

                EditBtn.IsVisible = false;
                SettingsBtn.IsVisible = true;

                SelectedNotebookBar.IsVisible = false;

                ListCtl.BackgroundColor = (Color)App.Current.Resources["NotebooksListBackgroundColor"]; ;
                ToolBar.BackgroundColor = (Color)App.Current.Resources["NotebooksToolBarBackgroundColor"]; ;

                ListCtlSetItemsSource(App.NotebooksManager.Notebooks);

                if (App.NotebooksManager.PreviouslySelectedNotebook != null)
                    ListCtlScrollTo(App.NotebooksManager.PreviouslySelectedNotebook);
            }
            else
            {
                BackBtn.IsVisible = true;

                AppTitle.IsVisible = false;
                AppTitle.Margin = new Thickness(0, 0, 0, 0);

                EditBtn.IsVisible = true;
                SettingsBtn.IsVisible = false;

                SelectedNotebookBar.IsVisible = true;
                SelectedNotebookName.Text = App.NotebooksManager.SelectedNotebook.NameForLists;
                SelectedNotebookStorageName.Text = App.NotebooksManager.SelectedNotebook.Storage?.Name;

                ListCtl.BackgroundColor = (Color)App.Current.Resources["PagesListBackgroundColor"];
                ToolBar.BackgroundColor = (Color)App.Current.Resources["PagesToolBarBackgroundColor"];

                ListCtlSetItemsSource(App.NotebooksManager.SelectedNotebook.Items);

                Page pageToScroll = null;
                if (App.NotebooksManager.SelectedPage != null && App.NotebooksManager.SelectedPage.Notebook == App.NotebooksManager.SelectedNotebook)
                    pageToScroll = App.NotebooksManager.SelectedPage;
                //else
                //{
                //    if(App.NotebooksManager.SelectedNotebook.Items != null && App.NotebooksManager.SelectedNotebook.Items.Count > 0)
                //        pageToScroll = App.NotebooksManager.SelectedNotebook.Items?.First();
                //}
                if (pageToScroll != null)
                    ListCtlScrollTo(pageToScroll);
            }


            SearchQuery.BackgroundColor = ListCtl.BackgroundColor;

            BatchCommit();
        }


        //

        void PagesSorted(object sender, Notebook notebook)
        {
            Device.BeginInvokeOnMainThread(() => ListCtlSetItemsSource(App.NotebooksManager.SelectedNotebook?.Items));
        }

		void PageLoaded(object sender, Page page)
		{
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
            await Application.Current.MainPage.DisplayActionSheet("Sort...", T.Localized("Cancel"), null, "Name", "Name (descending)", "Date", "Date (descending)", "Color", "Priority");
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
                    {
                        App.NotebooksManager.SelectNotebookAsync(n, App.Settings.TryToUnlockItemChildren);
                        //ListCtl.ScrollTo(n, ScrollToPosition.MakeVisible, true);
                    }
                }
            }
            else
            {
                Page p = await App.NotebooksManager.SelectedNotebook.NewPageAsync();
                if (p != null)
                {
                    await App.NotebooksManager.SelectPageAsync(p, App.Settings.TryToUnlockItemChildren);
                    Device.BeginInvokeOnMainThread(() => ListCtlScrollTo(p));
                }
            }
        }


        void EditItemsBtn_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Edit items...", "edit multiple items", "Cancel");
        }

    }
}
