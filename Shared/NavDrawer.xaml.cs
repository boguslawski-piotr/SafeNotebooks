using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class NavDrawer : ContentPageWAppBar
	{
		public Notebook SelectedNotebook = null;

		public NavDrawer()
		{
			InitializeComponent();

			ListCtl.ItemSelected += ListCtl_ItemSelected;
			ListCtl.ItemTapped += ListCtl_ItemTapped;

			MessagingCenter.Subscribe<MainFrame, bool>(this, MainFrame.MsgNavDrawerVisibilityChanged, NavDrawerVisibilityChanged);
		}

		protected override void OnAppearing()
		{
			ShowNotebooks();
		}

		protected override void AdjustAppBar(bool IsLandscape)
		{
			AdjustAppBar(IsLandscape, Grid, AppBar, true);
		}

		protected override void AdjustContent(bool IsLandscape)
		{
			SelectedNotebookBar.HeightRequest = (IsLandscape ? Metrics.ToolBarHeightLandscape : Metrics.ToolBarHeightPortrait);
			SelectedNotebookBar.Padding = new Thickness(
				Metrics.ScreenEdgeMargin,
				0,
				Metrics.ScreenEdgeMargin,
				0);
		}

		protected override void AdjustToolBar(bool IsLandscape)
		{
			AdjustToolBar(IsLandscape, Grid, ToolBar);
		}


		public void NavDrawerVisibilityChanged(SafeNotebooks.MainFrame MainFrame, bool IsVisible)
		{
		}


		void SearchBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Search...", "Window for search in all data.", "Cancel");
		}

		void SettingsBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Settings...", "Window for setting different program options.", "Cancel");
		}


		void ShowNotebooksBtn_Clicked(object sender, System.EventArgs e)
		{
			ShowNotebooks();
		}

		void ShowNotebooks()
		{
			ListCtl.ItemsSource = App.Data.Notebooks;
			SelectedNotebookName.Text = "Notebooks";	// TODO: translation
			ShowNotebooksBtn.IsVisible = false;
			SelectedNotebook = null;
		}


		void ListCtl_ItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if(e.SelectedItem is Notebook)
				SelectedNotebook = (Notebook)e.SelectedItem;
		}

		void ListCtl_ItemTapped(object sender, ItemTappedEventArgs e)
		{
			if (e.Item is Notebook)
			{
				ShowNotebook((Notebook)e.Item);
			}
			else
			{
				ShowPage((Page)e.Item);
			}
		}

		void ShowNotebook(Notebook n)
		{
			SelectedNotebook = n;

			if (SelectedNotebook == null) 
			{
				ShowNotebooks();
			}
			else
			{
				ListCtl.ItemsSource = SelectedNotebook.Pages;
				SelectedNotebookName.Text = SelectedNotebook.Name;

				ShowNotebooksBtn.IsVisible = true;
			}
		}

		void ShowPage(Page p)
		{
			Page SelectedPage = p;
			ListCtl.SelectedItem = p;

			MessagingCenter.Send<Data, Page>(App.Data, Data.MsgPageSelected, SelectedPage);
			MessagingCenter.Send<Xamarin.Forms.Page>(this, MainFrame.MsgHideNavDrawer);
		}

		async void NewBtn_Clicked(object sender, System.EventArgs e)
		{
			if (SelectedNotebook == null)
			{
				Notebook n = new Notebook()
				{
					Name = "Notebook " + App.Data.Notebooks.Count
				};

				await Application.Current.MainPage.DisplayAlert("New notebook", "Window for creating new notebook.", "Cancel");

				App.Data.Notebooks.Add(n);

				ShowNotebook(n);
			}
			else
			{
				Page p = new Page()
				{
					Name = "Page " + SelectedNotebook.Pages.Count
				};

				SelectedNotebook.Pages.Add(p);

				ShowPage(p);
			}
		}

	}
}
