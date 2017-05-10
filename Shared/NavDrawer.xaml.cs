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
			ToolBar.HeightRequest = (IsLandscape? Metrics.AppBarHeightLandscape : Metrics.AppBarHeightPortrait);
			ToolBar.Padding = new Thickness(
				Metrics.ScreenEdgeLeftRightMargin,
				0,
				Metrics.ScreenEdgeLeftRightMargin,
				0);
		}

		void SearchBtn_Clicked(object sender, System.EventArgs e)
		{
			Search.IsVisible = !Search.IsVisible;
			if (Search.IsVisible)
				Search.Focus();
		}
	}
}
