using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class NotebooksView : ContentView
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

		Size _osa;
		
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
			
            if (!pbXNet.Tools.IsDifferent(new Size(width, height), ref _osa))
				return;

			if (_Grid != null)
                ContentPageEx.LayoutAppBarAndToolBar(width, height, _Grid, _AppBarRow, _ToolBarRow);
        }

        //

        void ShowSelectedNotebook()
        {
            if (App.Data.SelectedNotebook == null)
            {
                BackBtn.IsVisible = false;

                ListCtl.ItemsSource = App.Data.Notebooks;

                SelectedNotebookName.Text = "Safe Notebooks";    // TODO: translation
                SelectedNotebookName.Margin = new Thickness(Metrics.ToolBarItemsWideSpacing, 0, 0, 0);
            }
            else
            {
                BackBtn.IsVisible = true;

                ListCtl.ItemsSource = App.Data.SelectedNotebook.Pages;

                SelectedNotebookName.Text = App.Data.SelectedNotebook.DisplayName;
                SelectedNotebookName.Margin = new Thickness(0);
            }
        }

        //

        void BackBtn_Clicked(object sender, System.EventArgs e)
        {
            App.Data.SelectNotebook(null);
        }

        void EditBtn_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Edit...", "Enable multiple items edit/delete mode?", "Cancel");
        }

        void SearchBtn_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Search...", "Window for search in all data.", "Cancel");
        }

        async void SettingsBtn_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new SettingsWnd(), true);
        }


        async void EditItemBtn_Clicked(object sender, System.EventArgs e)
        {
            Item item = (Item)(sender as MenuItem).CommandParameter;
            await Application.Current.MainPage.DisplayAlert("Edit", item.ToString(), "Cancel");
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
