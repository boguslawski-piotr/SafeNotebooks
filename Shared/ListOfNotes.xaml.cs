using System;
using System.Collections.Generic;

using Xamarin.Forms;
using pbXForms;
using System.Threading.Tasks;

namespace SafeNotebooks
{
	public partial class ListOfNotes : ContentPageWAppBar
	{
		Page Page = null;

		public ListOfNotes()
		{
			InitializeComponent();

			MessagingCenter.Subscribe<Data, Page>(this, Data.MsgPageSelected, PageSelected);
			MessagingCenter.Subscribe<MainFrame, bool>(this, MainFrame.MsgNavDrawerVisibilityChanged, NavDrawerVisibilityChanged);
		}

		protected override void OnAppearing()
		{
		}


		protected override void AdjustAppBar(bool IsLandscape)
		{
#if __IOS__
			bool StatusBarVisible = !IsLandscape || Device.Idiom == TargetIdiom.Tablet;
#endif
#if __ANDROID__
			bool StatusBarVisible = false;
#endif
			AppBar.HeightRequest = (IsLandscape? Metrics.AppBarHeightLandscape : Metrics.AppBarHeightPortrait);
			AppBar.Padding = new Thickness(
				Metrics.ScreenEdgeLeftRightMargin,
				StatusBarVisible? Metrics.StatusBarHeight : 0,
				Metrics.ScreenEdgeLeftRightMargin,
				0);
		}

		protected override void AdjustToolBar(bool IsLandscape)
		{
			ToolBar.HeightRequest = (IsLandscape ? Metrics.ToolBarHeightLandscape : Metrics.ToolBarHeightPortrait);
			ToolBar.Padding = new Thickness(
				Metrics.ScreenEdgeLeftRightMargin,
				0,
				Metrics.ScreenEdgeLeftRightMargin,
				0);
		}


		public void NavDrawerVisibilityChanged(MainFrame MainFrame, bool IsVisible)
		{
#if __IOS__
			//NavDrawerBtn.IsVisible = !IsVisible;
#endif
		}

		void NavDrawerBtn_Clicked(object sender, System.EventArgs e)
		{
			MessagingCenter.Send<Xamarin.Forms.Page>(this, MainFrame.MsgChangeNavDrawerVisibility);
		}


		void PageSelected(SafeNotebooks.Data obj, Page p)
		{
			Page = p;
			PageName.Text = Page.ToString() ; //+ " in " + App.Data.Notebook.ToString();
			ListCtl.ItemsSource = Page.Items;
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

			Page.Items.Add(i);
		}
	}
}
