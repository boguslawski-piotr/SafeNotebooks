using System;
using System.Collections.Generic;

using Xamarin.Forms;
using pbXForms;
using System.Threading.Tasks;

namespace SafeNotebooks
{
	public partial class ListOfNotes : ContentPageWAppBar
	{
		Page SelectedPage = null;

		public ListOfNotes()
		{
			InitializeComponent();

			MessagingCenter.Subscribe<Data, Page>(this, Data.MsgPageSelected, PageSelected);
			MessagingCenter.Subscribe<MainFrame, bool>(this, MainFrame.MsgNavDrawerVisibilityChanged, NavDrawerVisibilityChanged);
		}


		protected override void OnAppearing()
		{
			PageSelected(App.Data, SelectedPage);
		}


		protected override void AdjustAppBar(bool IsLandscape)
		{
			AdjustAppBar(IsLandscape, Grid, AppBar,
#if __IOS__
			             true
#endif
#if __ANDROID__
						 false
#endif
						);
		}

		protected override void AdjustContent(bool IsLandscape)
		{
		}

		protected override void AdjustToolBar(bool IsLandscape)
		{
            AdjustToolBar(IsLandscape, Grid, ToolBar);
		}


		void NavDrawerBtn_Clicked(object sender, System.EventArgs e)
		{
			MessagingCenter.Send<Xamarin.Forms.Page>(this, MainFrame.MsgShowNavDrawer);
		}

		public void NavDrawerVisibilityChanged(MainFrame MainFrame, bool IsVisible)
		{
#if __IOS__
			NavDrawerBtn.IsVisible = !IsVisible;
#endif
		}


		void PageSelected(SafeNotebooks.Data obj, Page p)
		{
			SelectedPage = p;
			if (SelectedPage != null)
			{
				SelectedPageName.Text = SelectedPage.ToString(); //+ " in " + App.Data.Notebook.ToString();
				EditBtn.IsVisible = true;
				SortBtn.IsVisible = true;
				ListCtl.ItemsSource = SelectedPage.Items;
				ListCtl.IsVisible = true;
				ToolBar.IsVisible = true;
			}
			else
			{
				SelectedPageName.Text = "";
				EditBtn.IsVisible = false;
				SortBtn.IsVisible = false;
				ListCtl.ItemsSource = null;
				ListCtl.IsVisible = false;
				ToolBar.IsVisible = false;
				MessagingCenter.Send<Xamarin.Forms.Page>(this, MainFrame.MsgShowNavDrawer);
			}
		}

		void ListCtl_Refreshing(object sender, EventArgs e)
		{
			ListCtl.EndRefresh();
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
			Note i = new Note()
			{
				Name = DateTime.Now.ToString()
			};

			SelectedPage.Items.Add(i);
		}
	}
}
