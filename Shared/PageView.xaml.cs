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

			Wnd.C.DetailViewWillBeShown += DetailViewWillBeShown;

			App.C.NotebooksManager.PageWillBeSelected += PageWillBeSelected;
			App.C.NotebooksManager.ItemObservableItemsCreated += ItemObservableItemsCreated;
			App.C.NotebooksManager.PageSelected += PageSelected;
			App.C.NotebooksManager.PageLoaded += PageLoaded;

			ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null; // disable item selection
			ListCtl.ItemTapped += ListCtl_ItemTapped;

			InitializeSearchBarFor(ListCtl);
		}

		protected override void ContinueOnSizeAllocated(double width, double height)
		{
			BackBtn.IsVisible = !Wnd.C.IsSplitView;

			double m = !BackBtn.IsVisible ? Metrics.ScreenEdgeMargin : 0;
			SelectedPageName.Margin = new Thickness(m, 0, 0, 0);
			SelectedPageParentName.Margin = new Thickness(m, 0, 0, 0);

			if (Wnd.C.IsSplitView && App.C.NotebooksManager.SelectedPage == null)
				UI(forPage: false);
			else
				UI(forPage: true);
		}


		//

		void DetailViewWillBeShown(object sender, (View view, object param) e)
		{
			if (e.param is Page page && page != App.C.NotebooksManager.SelectedPage)
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
			//Device.BeginInvokeOnMainThread(() => AIIsVisible(false));
		}

		void PageLoaded(object sender, Page page)
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
				Wnd.C.MasterViewIsVisible = true;
			}
		}

		void SetPageName(Page page)
		{
			SelectedPageName.Text = page?.NameForLists;
			SelectedPageParentName.Text = $"{Localized.T("in")} {page?.Notebook?.NameForLists}, {page?.Notebook?.Storage?.Name}";
		}

		void UI(bool forPage)
		{
			BatchBegin();

			AppBarLayout.IsVisible = forPage;
			ListCtl.IsVisible = forPage;
			ToolBarLayout.IsVisible = forPage;

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
				Wnd.C.ShowDetailViewAsync<TestView>(MastersDetailsPage.ViewsSwitchingAnimation.Forward);
				//App.C.NotebooksManager.SelectNoteAsync(note, App.C.Settings.TryToUnlockItemChildren);
			}
		}


		//

		public override async void OnSwipeLeftToRight()
		{
			if (!Wnd.C.IsSplitView)
				await Wnd.C.ShowMasterViewAsync<NotebookView>(MastersDetailsPage.ViewsSwitchingAnimation.Back, App.C.NotebooksManager.SelectedNotebook);
		}

		void BackBtn_Clicked(object sender, System.EventArgs e)
		{
			OnSwipeLeftToRight();
		}


		//

		void EditBtn_Clicked(object sender, System.EventArgs e)
		{
			App.C.NotebooksManager.SelectedPage?.EditAsync();
		}


		//

		public override void SearchQueryChanged(string text)
		{
		}


		//

		void SortBtn_Clicked(object sender, System.EventArgs e)
		{
			base.SortBtn_Clicked(Localized.T("Notes"), App.C.NotebooksManager.SelectedPage, ListCtl, true);
		}

		async void NewBtn_Clicked(object sender, System.EventArgs e)
		{
			string rc = await Application.Current.MainPage.DisplayActionSheet(Localized.T("SelectAndNew"), Localized.T("Cancel"), null, "Note", "Checklist", "Secret");
			New(rc);
		}

		void FavoriteNewBtn_Clicked(object sender, System.EventArgs e)
		{
			New("Note");
		}

		async void New(string what)
		{
			//for (int i = 0; i < 400; i++)
			//{
			//	await App.C.NotebooksManager.SelectedPage.NewNoteAsync();
			//}
			Note o = await App.C.NotebooksManager.SelectedPage.NewNoteAsync();
			if (o != null)
			{
				await App.C.NotebooksManager.SelectNoteAsync(o);
				Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollTo(ListCtl, o));
			}
		}

		void ToogleSelectModeBtn_Clicked(object sender, System.EventArgs e)
		{
			if (App.C.NotebooksManager.SelectedPage != null)
				App.C.NotebooksManager.SelectedPage.SelectModeForItemsEnabled = !App.C.NotebooksManager.SelectedPage.SelectModeForItemsEnabled;
		}
	}
}
