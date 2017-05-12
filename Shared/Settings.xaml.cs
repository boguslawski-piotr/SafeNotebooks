using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class Settings : ContentPageWAppBar
	{
		public Settings()
		{
			InitializeComponent();
		}


		protected override void AdjustAppBar(bool IsLandscape)
		{
			AdjustAppBar(IsLandscape, Grid, AppBar, Device.RuntimePlatform == Device.iOS);
		}


		void ReturnBtn_Clicked(object sender, System.EventArgs e)
		{
			Navigation.PopModalAsync();
		}
	}
}
