using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class NavDrawer : ContentPage
	{
		public NavDrawer()
		{
			InitializeComponent();

		}

		//protected override void OnSizeAllocated(double width, double height)
		//{
		//	base.OnSizeAllocated(width, height);

		//	Rectangle AppRect = Application.Current.MainPage.Bounds;
		//	SetPadding(AppRect.Width > AppRect.Height);
		//}

//		private void SetPadding(bool IsLandscape)
//		{
//#if __IOS__
//			bool StatusBarVisible = !IsLandscape || Device.Idiom == TargetIdiom.Tablet;
//			Padding = new Thickness(0, StatusBarVisible ? 20 : 0, 0, 0);
//#endif
//		}

		void SearchBtn_Clicked(object sender, System.EventArgs e)
		{
			Search.IsVisible = !Search.IsVisible;
			if (Search.IsVisible)
				Search.Focus();
		}
	}
}
