using System;
using System.Collections.Generic;
using System.Diagnostics;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class PageView : BaseView
    {
        public PageView()
        {
            InitializeComponent();

            MainWnd.Current.DetailViewWillBeShown += DetailViewWillBeShown;

            App.NotebooksManager.PageWillBeSelected += PageWillBeSelected;
            App.NotebooksManager.ItemObservableItemsCreated += ItemObservableItemsCreated;
            App.NotebooksManager.PageSelected += PageSelected;

            ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null; // disable item selection
			ListCtl.ItemTapped += ListCtl_ItemTapped;

            InitializeSearchBarFor(ListCtl);
		}

        protected override void ContinueOnSizeAllocated(double width, double height)
        {
            BackBtn.IsVisible = !MainWnd.Current.IsSplitView;

            double m = !BackBtn.IsVisible ? Metrics.ScreenEdgeMargin : 0;
            SelectedPageName.Margin = new Thickness(m, 0, 0, 0);
            SelectedPageParentName.Margin = new Thickness(m, 0, 0, 0);

            if (MainWnd.Current.IsSplitView && App.NotebooksManager.SelectedPage == null)
                UI(forPage: false);
            else
                UI(forPage: true);
        }


        //

        void DetailViewWillBeShown(object sender, (View view, object param) e)
        {
            if (e.param is Page page && page != App.NotebooksManager.SelectedPage)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ListCtl.ItemsSource = null;
                    SetPageName(page);
                    AIIsVisible(true);
                });
            }
        }

        void PageWillBeSelected(object sender, Page page)
        {
            Device.BeginInvokeOnMainThread(() => ShowPage(page));
        }

        void ItemObservableItemsCreated(object sender, ItemWithItems forWhom)
        {
            if (forWhom is Page page)
                Device.BeginInvokeOnMainThread(() => ListCtl.ItemsSource = page.ObservableItems);
        }

        void PageSelected(object sender, Page page)
        {
            Device.BeginInvokeOnMainThread(() => AIIsVisible(false));
        }


        //

        void ShowPage(Page page)
        {
            if (page != null)
            {
                ListCtl.ItemsSource = page.ObservableItems;
                SetPageName(page);
                UI(forPage: true);
            }
            else
            {
                UI(forPage: false);
                MainWnd.Current.MasterViewIsVisible = true;
            }
        }

        void SetPageName(Page page)
        {
            SelectedPageName.Text = page?.NameForLists;
            SelectedPageParentName.Text = $"{T.Localized("in")} {page?.Notebook?.NameForLists}, {page?.Notebook?.Storage?.Name}";
        }

        void UI(bool forPage)
        {
            BatchBegin();

            AppBar.IsVisible = forPage;
            ListCtl.IsVisible = forPage;
            ToolBar.IsVisible = forPage;

            NoUIBar.IsVisible = !forPage;

            BatchCommit();
        }

        void AIIsVisible(bool isVisible)
        {
            AI.IsVisible = isVisible;
            AI.IsRunning = isVisible;
        }


        //

        void ListCtl_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Note note = (Note)e.Item;
            if (note.Page.SelectModeForItemsEnabled)
                note.IsSelected = !note.IsSelected;
            else
            {
                MainWnd.Current.ShowDetailViewAsync<TestView>(MasterDetailPageEx.ViewsSwitchingAnimation.Forward);
                //App.NotebooksManager.SelectNoteAsync(note, App.Settings.TryToUnlockItemChildren);
            }
        }


        //

        async void BackBtn_Clicked(object sender, System.EventArgs e)
        {
            await MainWnd.Current.ShowMasterViewAsync<NotebookView>(MasterDetailPageEx.ViewsSwitchingAnimation.Back, App.NotebooksManager.SelectedNotebook);
		}

        void EditBtn_Clicked(object sender, System.EventArgs e)
        {
            App.NotebooksManager.SelectedPage?.EditAsync();
        }


		//

		public override void SearchQueryChanged(string text)
		{
		}

		
        //

        void SortBtn_Clicked(object sender, System.EventArgs e)
        {
			base.SortBtn_Clicked(T.Localized("Notes"), App.NotebooksManager.SelectedPage, ListCtl, true);
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
                Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollTo(ListCtl, o));
            }
        }

        void EditItemsBtn_Clicked(object sender, System.EventArgs e)
        {
            if (App.NotebooksManager.SelectedPage != null)
                App.NotebooksManager.SelectedPage.SelectModeForItemsEnabled = !App.NotebooksManager.SelectedPage.SelectModeForItemsEnabled;
        }
    }
}
