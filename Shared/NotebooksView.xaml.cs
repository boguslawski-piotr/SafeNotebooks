using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            App.NotebooksManager.NotebookSelected += (sender, notebook) => ShowSelectedNotebook();
            App.NotebooksManager.NotebookLoaded += NotebookLoaded;
            App.NotebooksManager.PageLoaded += PageLoaded;

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null;
            ListCtl.ItemTapped += (object sender, ItemTappedEventArgs e) =>
            {
                if (e.Item is Notebook)
                    App.NotebooksManager.SelectNotebookAsync((Notebook)e.Item, App.Settings.TryToUnlockItemChildren);
                else
                    App.NotebooksManager.SelectPageAsync((Page)e.Item, App.Settings.TryToUnlockItemChildren);
            };

            ShowSelectedNotebook();
        }

        protected override void ContinueOnSizeAllocated(double width, double height)
        {
        }

        //

        void ShowSelectedNotebook()
        {
            BatchBegin();

            if (App.NotebooksManager.SelectedNotebook == null)
            {
                BackBtn.IsVisible = false;

                AppTitle.Margin = new Thickness(Metrics.ScreenEdgeMargin, 0, 0, 0);

                SelectedNotebookBar.IsVisible = false;

                ListCtl.ItemsSource = App.NotebooksManager.Notebooks;

                NewBtn.Text = T.Localized("NewNotebook");
            }
            else
            {
                BackBtn.IsVisible = true;

                AppTitle.Margin = new Thickness(0, 0, 0, 0);

                SelectedNotebookBar.IsVisible = true;

                ListCtl.ItemsSource = App.NotebooksManager.SelectedNotebook.Pages;

                SelectedNotebookName.Text = App.NotebooksManager.SelectedNotebook.NameForLists;

                NewBtn.Text = T.Localized("NewPage");
            }

            BatchCommit();
        }

        void RefreshListCtl()
        {
            // TODO: How to refresh ListView in a more elegant way?
            var l = ListCtl.ItemsSource;
            ListCtl.BeginRefresh();
            ListCtl.ItemsSource = null;
            ListCtl.ItemsSource = l;
            ListCtl.EndRefresh();
        }

        void NotebookLoaded(object sender, Notebook notebook)
        {
            RefreshListCtl();
        }

        void PageLoaded(object sender, Page page)
        {
            RefreshListCtl();
        }

        //

        void SearchBtn_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Search...", "Window for search in all data.", "Cancel");
        }

        async void SettingsBtn_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new SettingsWnd(), true);
        }


        void BackBtn_Clicked(object sender, System.EventArgs e)
        {
            App.NotebooksManager.SelectNotebookAsync(null, false);
        }

        void EditBtn_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Edit...", "Enable multiple items edit/delete mode?", "Cancel");
        }


        async void EditItemBtn_Clicked(object sender, System.EventArgs e)
        {
            Item item = (Item)(sender as MenuItem).CommandParameter;
            await Application.Current.MainPage.DisplayAlert("Edit", item.NameForLists, "Cancel");
        }

        async void MoveItemBtn_Clicked(object sender, System.EventArgs e)
        {
            Item item = (Item)(sender as MenuItem).CommandParameter;
            await Application.Current.MainPage.DisplayAlert("Move", item.NameForLists, "Cancel");
        }

        async void DeleteItemBtn_Clicked(object sender, System.EventArgs e)
        {
            Item item = (Item)(sender as MenuItem).CommandParameter;
            await Application.Current.MainPage.DisplayAlert("Delete", item.NameForLists, "Cancel");
        }


		//

        public async Task<StorageOnFileSystem<string>> SelectStorageUIAsync(IEnumerable<StorageOnFileSystem<string>> storages)
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
                    App.NotebooksManager.SelectPageAsync(p, App.Settings.TryToUnlockItemChildren);
            }
        }

        void SortBtn_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Sort", "Select sort for current view (ask for default?)", "Cancel");
        }

    }
}
