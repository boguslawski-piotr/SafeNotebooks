using System;
using System.Collections.Generic;

using Xamarin.Forms;
using pbXForms;

namespace SafeNotebooks
{
	public partial class ListOfNotes : ContentPageWAppBar
	{
		public ListOfNotes()
		{
			InitializeComponent();

			MessagingCenter.Subscribe<MainFrame, bool>(this, App.MsgNavDrawerVisibilityChanged, NavDrawerVisibilityChanged);
		}

		protected override void AdjustAppBar(bool IsLandscape)
		{
			AppBar.HeightRequest = (IsLandscape? Metrics.AppBarHeightLandscape : Metrics.AppBarHeightPortrait);
		#if __IOS__
			bool StatusBarVisible = !IsLandscape || Device.Idiom == TargetIdiom.Tablet;
			AppBar.Padding = new Thickness(
				Metrics.ScreenEdgeLeftRightMargin, 
				StatusBarVisible? Metrics.StatusBarHeight : 0, 
				Metrics.ScreenEdgeLeftRightMargin,
				0);
		#endif
		#if __ANDROID__
		#endif
		}

		protected override void AdjustToolBar(bool IsLandscape)
		{
			ToolBar.HeightRequest = (IsLandscape ? Metrics.AppBarHeightLandscape : Metrics.AppBarHeightPortrait);
			ToolBar.Padding = new Thickness(
				Metrics.ScreenEdgeLeftRightMargin,
				0,
				Metrics.ScreenEdgeLeftRightMargin,
				0);
		}

		public void NavDrawerVisibilityChanged(SafeNotebooks.MainFrame MainFrame, bool IsVisible)
		{
			if (IsVisible)
				Search.IsVisible = false;
			
			NavDrawerBtn.IsVisible = !IsVisible;
		}

		void SearchBtn_Clicked(object sender, System.EventArgs e)
		{
			Search.IsVisible = !Search.IsVisible;
			if (Search.IsVisible)
				Search.Focus();
		}
	}
}
