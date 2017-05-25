using System;
using System.Collections.Generic;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class PageView : ContentViewEx
    {
        public PageView()
        {
            InitializeComponent();

            App.NotebooksManager.PageSelected += (sender, page) => ShowSelectedPage();

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null;
            //ListCtl.ItemTapped += (object sender, ItemTappedEventArgs e) => App.Data.SelectNote((Note)e.Item);

            NoUI();
        }

        protected override void ContinueOnSizeAllocated(double width, double height)
        {
            BackBtn.IsVisible = !MainWnd.Current.IsSplitView;

            double m = !BackBtn.IsVisible ? Metrics.ScreenEdgeMargin : 0;
            SelectedPageName.Margin = new Thickness(m, 0, 0, 0);
            SelectedPageParentName.Margin = new Thickness(m, 0, 0, 0);
        }

        void NoUI()
        {
            BatchBegin();

            AppBar.IsVisible = false;

			ListCtl.IsVisible = false;
			ListCtl.ItemsSource = null;
            NoUIBar.IsVisible = true;

            ToolBar.IsVisible = false;

            BatchCommit();
        }

        void ShowSelectedPage()
        {
            if (App.NotebooksManager.SelectedPage != null)
            {
                BatchBegin();

                AppBar.IsVisible = true;

                SelectedPageName.Text = App.NotebooksManager.SelectedPage.NameForLists;
                SelectedPageParentName.Text = $"{T.Localized("in")} {App.NotebooksManager.SelectedPage.Notebook?.NameForLists}, {App.NotebooksManager.SelectedPage.Notebook?.Storage?.Name}";

                ListCtl.ItemsSource = App.NotebooksManager.SelectedPage.ObservableItems;
                ListCtl.IsVisible = true;
                NoUIBar.IsVisible = false;

				ToolBar.IsVisible = true;

                MainWnd.Current.NotebooksViewIsVisible = false;

                BatchCommit();
            }
            else
            {
                NoUI();
                MainWnd.Current.NotebooksViewIsVisible = true;
            }
        }


        void BackBtn_Clicked(object sender, System.EventArgs e)
        {
            ListCtl.ItemsSource = null;
            MainWnd.Current.NotebooksViewIsVisible = true;
        }

        void EditBtn_Clicked(object sender, System.EventArgs e)
        {
			App.NotebooksManager.SelectedPage?.EditAsync();
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


		void SortBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Sort", "Select sort for current view (ask for default?)", "Cancel");
		}

		async void NewBtn_Clicked(object sender, System.EventArgs e)
        {
            string rc = await Application.Current.MainPage.DisplayActionSheet(T.Localized("SelectAndNew"), T.Localized("Cancel"), null, "Note", "Checklist", "Secret");
            New(rc);
        }

        void FavoriteNewBtn_Clicked(object sender, System.EventArgs e)
        {
            New("Note");
        }

        async void New(string what)
        {
            Note o = await App.NotebooksManager.SelectedPage.NewNoteAsync();
            if (o != null)
            {
                App.NotebooksManager.SelectNoteAsync(o);
                ListCtl.ScrollTo(o, ScrollToPosition.MakeVisible, true);
            }
        }

        void EditItemsBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Edit items...", "edit multiple items", "OK", "Cancel");
		}


	}
}
