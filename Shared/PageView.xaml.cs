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

            App.Data.PageSelected += (sender, page) => ShowSelectedPage();

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null;
            //ListCtl.ItemTapped += (object sender, ItemTappedEventArgs e) => App.Data.SelectNote((Note)e.Item);

            //NoDataUI();
        }

        protected override void ContinueOnSizeAllocated(double width, double height)
        {
            BackBtn.IsVisible = !MainWnd.Current.IsSplitView;

            double m = !BackBtn.IsVisible ? Metrics.ScreenEdgeMargin : 0;
            SelectedPageName.Margin = new Thickness(m, 0, 0, 0);
            SelectedPageParentName.Margin = new Thickness(m, 0, 0, 0);
        }

        void NoDataUI()
        {
            BatchBegin();

            AppBar.IsVisible = false;

            ListCtl.ItemsSource = null;
            ListCtl.IsVisible = false;

            ToolBar.IsVisible = false;

            BatchCommit();
        }

        void ShowSelectedPage()
        {
            if (App.Data.SelectedPage != null)
            {
                BatchBegin();

                AppBar.IsVisible = true;

                SelectedPageName.Text = App.Data.SelectedPage.DisplayName;
                SelectedPageParentName.Text = $"{T.Localized("in")} {App.Data.SelectedPage.Parent.DisplayName}";

                ListCtl.ItemsSource = App.Data.SelectedPage.Notes;
                ListCtl.IsVisible = true;

                FavoriteNewBtn.Text = T.Localized("New") + ": " + "@Note@"; // TODO: get favorite item type name from page
                ToolBar.IsVisible = true;

                MainWnd.Current.NotebooksViewIsVisible = false;

                BatchCommit();
            }
            else
            {
                NoDataUI();
                MainWnd.Current.NotebooksViewIsVisible = true;
            }
        }


        void BackBtn_Clicked(object sender, System.EventArgs e)
        {
            MainWnd.Current.NotebooksViewIsVisible = true;
        }

        void EditBtn_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Edit...", "Enable multiple items edit/delete mode?", "Cancel");
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


        async void NewBtn_Clicked(object sender, System.EventArgs e)
        {
            string rc = await Application.Current.MainPage.DisplayActionSheet(T.Localized("SelectAndNew"), T.Localized("Cancel"), null, "Note", "Task", "Account", "Identity");
            New(rc);
        }

        void FavoriteNewBtn_Clicked(object sender, System.EventArgs e)
        {
            New("Note");
        }

        void New(string what)
        {
            //Application.Current.MainPage.DisplayAlert("Create new", what, "Cancel");
            Note note = new Note()
            {
                Name = DateTime.Now.ToString()
            };

            App.Data.SelectedPage.AddNote(note);
        }

        void SortBtn_Clicked(object sender, System.EventArgs e)
        {
            //Application.Current.MainPage.DisplayAlert("Sort", "Select sort for current view (ask for default?)", "Cancel");
            Application.Current.MainPage.Navigation.PushModalAsync(new UnlockWnd(), false);
        }

    }
}
