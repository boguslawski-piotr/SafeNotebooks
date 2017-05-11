using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class MainFrame : MasterDetailPage
	{
		public MainFrame()
		{
			InitializeComponent();

			IsPresentedChanged += NavDrawerVisibilityChanged;
			//IsPresented = true;

			MessagingCenter.Subscribe<Page>(this, App.MsgChangeNavDrawerVisibility, ChangeNavDrawerVisibility);
		}

		void NavDrawerVisibilityChanged(object sender, EventArgs e)
		{
			// TODO: hide keyboard (how?)
			MessagingCenter.Send<MainFrame, bool>(this, App.MsgNavDrawerVisibilityChanged, IsPresented);
		}

		void ChangeNavDrawerVisibility(Page obj)
		{
			IsPresented = !IsPresented;
		}
	}
}
