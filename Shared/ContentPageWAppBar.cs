using System;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
	abstract public class ContentPageWAppBar : ContentPage
	{
		public ContentPageWAppBar()
		{
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			Rectangle AppRect = Application.Current.MainPage.Bounds;

			AdjustAppBar(AppRect.Width > AppRect.Height);
            AdjustContent(AppRect.Width > AppRect.Height);
			AdjustToolBar(AppRect.Width > AppRect.Height);

			base.OnSizeAllocated(width, height);
		}

		abstract protected void AdjustAppBar(bool IsLandscape);

		protected void AdjustAppBar(bool IsLandscape, Grid Grid, Layout<View> AppBar, bool AppBarOverStatusBar)
		{
#if __IOS__
			bool StatusBarVisible = !IsLandscape || Device.Idiom == TargetIdiom.Tablet;
#endif
#if __ANDROID__
			bool StatusBarVisible = AppBarOverStatusBar;
#endif
			Grid.RowDefinitions[0].Height = (IsLandscape? Metrics.AppBarHeightLandscape : Metrics.AppBarHeightPortrait) + (StatusBarVisible? Metrics.StatusBarHeight : 0);

			AppBar.Padding = new Thickness(
				Metrics.ScreenEdgeMargin,
				StatusBarVisible? Metrics.StatusBarHeight : 0,
				Metrics.ScreenEdgeMargin,
							0);
		}

		abstract protected void AdjustContent(bool IsLandscape);

		abstract protected void AdjustToolBar(bool IsLandscape);

		protected void AdjustToolBar(bool IsLandscape, Grid Grid, Layout<View> ToolBar)
		{
			Grid.RowDefinitions[2].Height = (IsLandscape ? Metrics.ToolBarHeightLandscape : Metrics.ToolBarHeightPortrait);

			ToolBar.Padding = new Thickness(
				Metrics.ScreenEdgeMargin,
				0,
				Metrics.ScreenEdgeMargin,
				0);
		}

	}
}
