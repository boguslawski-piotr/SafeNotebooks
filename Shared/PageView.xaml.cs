using System;
using System.Collections.Generic;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class PageView : ContentView
    {
        public PageView()
        {
            InitializeComponent();

            App.Data.PageSelected += (sender, page) => ShowSelectedPage();

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null;
            //ListCtl.ItemTapped += (object sender, ItemTappedEventArgs e) => App.Data.SelectNote((Note)e.Item);

            NoDataUI();
        }

		Size _osa;
		
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

			if (!Tools.IsDifferent(new Size(width, height), ref _osa))
				return;
			
            if (_Grid != null)
            {
				ContentPageEx.LayoutAppBarAndToolBar(width, height, _Grid, _AppBarRow, _ToolBarRow);

				BackBtn.IsVisible = !MainWnd.Current.IsSplitView;

                double m = !BackBtn.IsVisible ? Metrics.ToolBarItemsWideSpacing : 0;
                SelectedPageName.Margin = new Thickness(m, 0, m, 0);
                SelectedPageParentName.Margin = new Thickness(m, 0, m, 0);
            }
        }

        void NoDataUI()
        {
            BatchBegin();

            _AppBarRow.IsVisible = false;

            ListCtl.ItemsSource = null;
            ListCtl.IsVisible = false;

            _ToolBarRow.IsVisible = false;

            BatchCommit();
        }

        void ShowSelectedPage()
        {
            if (App.Data.SelectedPage != null)
            {
				BatchBegin();
				
                _AppBarRow.IsVisible = true;

                SelectedPageName.Text = App.Data.SelectedPage .DisplayName;
                SelectedPageParentName.Text = "in " /* TODO: translation */ + App.Data.SelectedPage.Parent.DisplayName;

                ListCtl.ItemsSource = App.Data.SelectedPage .Notes;
                ListCtl.IsVisible = true;

                _ToolBarRow.IsVisible = true;

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


        async void DeleteItemBtn_Clicked(object sender, System.EventArgs e)
        {
            Item item = (Item)(sender as MenuItem).CommandParameter;
            await Application.Current.MainPage.DisplayAlert("Delete", item.ToString(), "Cancel");
        }


        async void NewBtn_Clicked(object sender, System.EventArgs e)
        {
            string rc = await Application.Current.MainPage.DisplayActionSheet("New...", "Cancel", null, "Note", "Task", "Account", "Identity");
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
