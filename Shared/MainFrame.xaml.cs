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
			IsPresented = true;
		}

		void NavDrawerVisibilityChanged(object sender, EventArgs e)
		{
			MessagingCenter.Send<MainFrame, bool>(this, App.MsgNavDrawerVisibilityChanged, IsPresented);
		}

	}
}
