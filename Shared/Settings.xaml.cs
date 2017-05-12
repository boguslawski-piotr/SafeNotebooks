using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class Settings : pbXForms.ContentPageEx
	{
		public Settings()
		{
			InitializeComponent();
		}

		void ReturnBtn_Clicked(object sender, System.EventArgs e)
		{
			Navigation.PopModalAsync();
		}
	}
}
