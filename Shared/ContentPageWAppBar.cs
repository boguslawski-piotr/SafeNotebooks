using System;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public class ContentPageWAppBar : ContentPage
	{
		public ContentPageWAppBar()
		{
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			bool IsLandscape = (Tools.DeviceOrientation == DeviceOrientations.Landscape);

			AdjustAppBar(IsLandscape);

			base.OnSizeAllocated(width, height);
		}

		protected virtual void AdjustAppBar(bool IsLandscape)
		{
		}

		protected void AdjustAppBar(bool IsLandscape, Grid Grid, Layout<View> AppBar, bool AppBarOverStatusBar)
		{
#if __IOS__
			bool StatusBarVisible = !IsLandscape || Device.Idiom == TargetIdiom.Tablet;
#endif
#if __ANDROID__
			bool StatusBarVisible = AppBarOverStatusBar;
#endif

			Grid.RowDefinitions[0].Height = (IsLandscape ? Metrics.AppBarHeightLandscape : Metrics.AppBarHeightPortrait) + (StatusBarVisible ? Metrics.StatusBarHeight : 0);

			AppBar.Padding = new Thickness(
				0,
				StatusBarVisible ? Metrics.StatusBarHeight : 0,
				0,
				0);
		}

	}
}
