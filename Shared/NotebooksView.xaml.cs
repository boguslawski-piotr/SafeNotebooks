using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class NotebooksView : ContentViewEx
    {
        public NotebooksView()
        {
            InitializeComponent();

            App.Data.NotebookSelected += (sender, notebook) => ShowSelectedNotebook();

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null;

            ListCtl.ItemTapped += (object sender, ItemTappedEventArgs e) =>
            {
                if (e.Item is Notebook)
                    App.Data.SelectNotebook((Notebook)e.Item);
                else
                    App.Data.SelectPage((Page)e.Item);
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

            if (App.Data.SelectedNotebook == null)
            {
                BackBtn.IsVisible = false;

                AppTitle.Margin = new Thickness(Metrics.ScreenEdgeMargin, 0, 0, 0);

                SelectedNotebookBar.IsVisible = false;

                ListCtl.ItemsSource = App.Data.Notebooks;

                NewBtn.Text = "New: Notebook"; // TODO: translation
            }
            else
            {
                BackBtn.IsVisible = true;

                AppTitle.Margin = new Thickness(0, 0, 0, 0);

                SelectedNotebookBar.IsVisible = true;

                ListCtl.ItemsSource = App.Data.SelectedNotebook.Pages;

                SelectedNotebookName.Text = App.Data.SelectedNotebook.DisplayName;

                NewBtn.Text = "New: Page"; // TODO: translation
			}

            BatchCommit();
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
			App.Data.SelectNotebook(null);
		}

		void EditBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Edit...", "Enable multiple items edit/delete mode?", "Cancel");
		}

		
        async void EditItemBtn_Clicked(object sender, System.EventArgs e)
        {
            Item item = (Item)(sender as MenuItem).CommandParameter;
            await Application.Current.MainPage.DisplayAlert("Edit", item.ToString(), "Cancel");
        }

		async void MoveItemBtn_Clicked(object sender, System.EventArgs e)
		{
			Item item = (Item)(sender as MenuItem).CommandParameter;
			await Application.Current.MainPage.DisplayAlert("Move", item.ToString(), "Cancel");
		}
		
        async void DeleteItemBtn_Clicked(object sender, System.EventArgs e)
        {
            Item item = (Item)(sender as MenuItem).CommandParameter;
            await Application.Current.MainPage.DisplayAlert("Delete", item.ToString(), "Cancel");
        }

        void NewBtn_Clicked(object sender, System.EventArgs e)
        {
            if (App.Data.SelectedNotebook == null)
            {
                Notebook n = new Notebook()
                {
                    Name = "Notebook " + App.Data.Notebooks.Count
                };

                App.Data.Notebooks.Add(n);

                App.Data.SelectNotebook(n);
            }
            else
            {
                Page p = new Page()
                {
                    Name = "Page " + App.Data.SelectedNotebook.Pages.Count
                };

                App.Data.SelectedNotebook.AddPage(p);

                App.Data.SelectPage(p);
            }
        }

        void SortBtn_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Sort", "Select sort for current view (ask for default?)", "Cancel");
        }

    }
}
