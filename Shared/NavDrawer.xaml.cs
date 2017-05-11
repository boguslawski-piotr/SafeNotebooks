using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class NavDrawer : ContentPageWAppBar
	{
		public NavDrawer()
		{
			InitializeComponent();

			MessagingCenter.Subscribe<MainFrame, bool>(this, App.MsgNavDrawerVisibilityChanged, NavDrawerVisibilityChanged);
		}


		protected override void AdjustAppBar(bool IsLandscape)
		{
#if __IOS__
			bool StatusBarVisible = !IsLandscape || Device.Idiom == TargetIdiom.Tablet;
#endif
#if __ANDROID__
			//bool StatusBarVisible = (Device.Idiom == TargetIdiom.Tablet) ? !IsLandscape : true;
			bool StatusBarVisible = true;
#endif
			AppBar.HeightRequest = (IsLandscape ? Metrics.AppBarHeightLandscape : Metrics.AppBarHeightPortrait);
			AppBar.Padding = new Thickness(
				Metrics.ScreenEdgeLeftRightMargin,
				StatusBarVisible ? Metrics.StatusBarHeight : 0,
				Metrics.ScreenEdgeLeftRightMargin,
				0);
		}

		protected override void AdjustToolBar(bool IsLandscape)
		{
			ToolBar.HeightRequest = (IsLandscape? Metrics.ToolBarHeightLandscape : Metrics.ToolBarHeightPortrait);
			ToolBar.Padding = new Thickness(
				Metrics.ScreenEdgeLeftRightMargin,
				0,
				Metrics.ScreenEdgeLeftRightMargin,
				0);
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


		void NewNotebookBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Create new", "notebook", "Cancel");
		}

		void NewPageBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Create new", "page", "Cancel");
		}
	}
}
