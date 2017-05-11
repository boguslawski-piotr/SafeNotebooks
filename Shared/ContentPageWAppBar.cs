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
			base.OnSizeAllocated(width, height);

			Rectangle AppRect = Application.Current.MainPage.Bounds;

			AdjustAppBar(AppRect.Width > AppRect.Height);
            AdjustToolBar(AppRect.Width > AppRect.Height);
		}

		abstract protected void AdjustAppBar(bool IsLandscape);
		abstract protected void AdjustToolBar(bool IsLandscape);
	}
}
