using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class ListOfNotes : ContentPage
	{
		public ListOfNotes()
		{
			InitializeComponent();

			MessagingCenter.Subscribe<MainFrame>(this, "NavDrawerIsPresentedChanged", NavDrawer_IsPresentedChanged);
		}

		public void NavDrawer_IsPresentedChanged(SafeNotebooks.MainFrame MainFrame)
		{
			if (MainFrame.IsPresented)
				Search.IsVisible = false;
		}

		void SearchBtn_Clicked(object sender, System.EventArgs e)
		{
			Search.IsVisible = !Search.IsVisible;
		}
	}
}
