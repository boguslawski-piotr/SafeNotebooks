using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class NotebookView : BaseView
	{
		public NotebookView()
		{
			InitializeComponent();

			Wnd.C.MasterViewWillBeShown += MasterViewWillBeShown;

			App.C.NotebooksManager.NotebookWillBeSelected += NotebookWillBeSelected;
			App.C.NotebooksManager.ItemObservableItemsCreated += ItemObservableItemsCreated;
			App.C.NotebooksManager.NotebookSelected += NotebookSelected;

			ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null; // disable item selection
			ListCtl.ItemTapped += ListCtl_ItemTapped;

			InitializeSearchBarFor(ListCtl);
		}


		//

		void MasterViewWillBeShown(object sender, (View view, object param) e)
		{
			if (e.view == this && e.param is Notebook notebook)
			{
				if (notebook != App.C.NotebooksManager.SelectedNotebook)
				{
					Device.BeginInvokeOnMainThread(() =>
					{
						ListCtl.ItemsSource = null;
						SetNotebookName(notebook);
						AIIsVisible(true);
					});

				}
				else
					Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollTo(ListCtl, App.C.NotebooksManager.SelectedPage));
			}
		}

		void NotebookWillBeSelected(object sender, Notebook notebook)
		{
			Device.BeginInvokeOnMainThread(() => ShowNotebook(notebook));
		}

		void ItemObservableItemsCreated(object sender, ItemWithItems forWhom)
		{
			if (forWhom is Notebook notebook)
				Device.BeginInvokeOnMainThread(() => ListCtl.ItemsSource = notebook.ObservableItems);
		}

		void NotebookSelected(object sender, Notebook notebook)
		{
			Device.BeginInvokeOnMainThread(() => AIIsVisible(false));
		}

		void ShowNotebook(Notebook notebook)
		{
			if (notebook == null)
			{
				SelectedNotebookBar.IsVisible = false;
				EditBtn.IsVisible = false;

				ListCtl.ItemsSource = null;
			}
			else
			{
				SelectedNotebookBar.IsVisible = true;
				EditBtn.IsVisible = true;

				ListCtl.ItemsSource = notebook.ObservableItems;
				SetNotebookName(notebook);
			}
		}

		void SetNotebookName(Notebook notebook)
		{
			SelectedNotebookName.Text = notebook.NameForLists;
			SelectedNotebookStorageName.Text = notebook.Storage?.Name;
		}

		void AIIsVisible(bool isVisible)
		{
			AI.IsVisible = isVisible;
			AI.IsRunning = isVisible;
		}


		//

		async Task SelectPage(Page page)
		{
			await Wnd.C.ShowDetailViewAsync<PageView>(MastersDetailsPage.ViewsSwitchingAnimation.Forward, page);
			await App.C.NotebooksManager.SelectPageAsync(page, App.Settings.TryToUnlockItemItems);
		}

		async void ListCtl_ItemTapped(object sender, ItemTappedEventArgs e)
		{
			if (e.Item is Page page)
			{
				if (page.Notebook.SelectModeForItemsEnabled)
					page.IsSelected = !page.IsSelected;
				else
				{
					await SelectPage(page);
				}
			}
		}


		//

		public override async void OnSwipeLeftToRight()
		{
			await Wnd.C.ShowMasterViewAsync<NotebooksView>(MastersDetailsPage.ViewsSwitchingAnimation.Back);
		}

		void BackBtn_Clicked(object sender, System.EventArgs e)
		{
			OnSwipeLeftToRight();
		}


		//

		void EditBtn_Clicked(object sender, System.EventArgs e)
		{
			App.C.NotebooksManager.SelectedNotebook?.EditAsync();
		}


		//

		public override void SearchQueryChanged(string text)
		{
		}


		//

		void SortBtn_Clicked(object sender, System.EventArgs e)
		{
			base.SortBtn_Clicked(T.Localized("Pages"), App.C.NotebooksManager.SelectedNotebook, ListCtl);
		}


		async void NewBtn_Clicked(object sender, System.EventArgs e)
		{
			Page page = await App.C.NotebooksManager.SelectedNotebook.NewPageAsync();
			if (page != null)
			{
				Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollTo(ListCtl, page));
				await SelectPage(page);
			}
		}


		void ToogleSelectModeBtn_Clicked(object sender, System.EventArgs e)
		{
			if (App.C.NotebooksManager.SelectedNotebook != null)
				App.C.NotebooksManager.SelectedNotebook.SelectModeForItemsEnabled = !App.C.NotebooksManager.SelectedNotebook.SelectModeForItemsEnabled;
		}
	}
}
