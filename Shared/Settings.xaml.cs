using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SafeNotebooks
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
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
