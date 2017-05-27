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
            App.NotebooksManager.NotesSorted += NotesSorted;

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null;
			ListCtl.ItemTapped += ListCtl_ItemTapped;

			//NoUI();
        }

        protected override void ContinueOnSizeAllocated(double width, double height)
        {
            BackBtn.IsVisible = !MainWnd.Current.IsSplitView;

            double m = !BackBtn.IsVisible ? Metrics.ScreenEdgeMargin : 0;
            SelectedPageName.Margin = new Thickness(m, 0, 0, 0);
            SelectedPageParentName.Margin = new Thickness(m, 0, 0, 0);

            if (MainWnd.Current.IsSplitView && App.NotebooksManager.SelectedPage == null)
                NoUI();
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

				BatchCommit();
				
                MainWnd.Current.NotebooksViewIsVisible = false;
            }
            else
            {
                NoUI();
                MainWnd.Current.NotebooksViewIsVisible = true;
            }
        }

        void NotesSorted(object sender, Page page)
        {
            Device.BeginInvokeOnMainThread(() => ViewsCommonLogic.ListViewSetItemsSource(ListCtl, App.NotebooksManager.SelectedPage?.ObservableItems));
        }


        void ListCtl_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Note note = (Note)e.Item;
            if (note.Page.SelectModeForItemsEnabled)
                note.IsSelected = !note.IsSelected;
            else
                App.NotebooksManager.SelectNoteAsync(note, App.Settings.TryToUnlockItemChildren);
        }


        void BackBtn_Clicked(object sender, System.EventArgs e)
        {
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


        async void SortBtn_Clicked(object sender, System.EventArgs e)
        {
            if (App.NotebooksManager.SelectedPage == null)
                return;

            ItemWithItems.SortParameters sortParams = App.NotebooksManager.SelectedPage.SortParameters;
            string title = T.Localized("HowToSort") + " " + T.Localized("Notes") + "?";

            SortParametersDlg d = new SortParametersDlg(title, sortParams);
            bool rc = await MainWnd.Current.ModalViewsManager.DisplayModalAsync(d, ModalViewsManager.ModalPosition.BottomCenter);
            if (rc)
            {
                App.NotebooksManager.SelectedPage.SortParameters = d.SortParams;
                App.NotebooksManager.SelectedPage.SortItems();
            }
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
                await App.NotebooksManager.SelectNoteAsync(o);
                Device.BeginInvokeOnMainThread(() => ViewsCommonLogic.ListViewScrollTo(ListCtl, o));
            }
        }

        void EditItemsBtn_Clicked(object sender, System.EventArgs e)
        {
            if (App.NotebooksManager.SelectedPage != null)
                App.NotebooksManager.SelectedPage.SelectModeForItemsEnabled = !App.NotebooksManager.SelectedPage.SelectModeForItemsEnabled;
        }


    }
}
