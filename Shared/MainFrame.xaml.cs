using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class MainFrame : MasterDetailPage
	{
		public static string MsgNavDrawerVisibilityChanged = "MsgNavDrawerVisibilityChanged";
		public static string MsgChangeNavDrawerVisibility = "MsgChangeNavDrawerVisibility";

		public MainFrame()
		{
			InitializeComponent();

			IsPresentedChanged += NavDrawerVisibilityChanged;

			IsPresented = true;

			MessagingCenter.Subscribe<Xamarin.Forms.Page>(this, MainFrame.MsgChangeNavDrawerVisibility, ChangeNavDrawerVisibility);
		}

		void NavDrawerVisibilityChanged(object sender, EventArgs e)
		{
			MessagingCenter.Send<MainFrame, bool>(this, MainFrame.MsgNavDrawerVisibilityChanged, IsPresented);
		}

		void ChangeNavDrawerVisibility(Xamarin.Forms.Page obj)
		{
			IsPresented = !IsPresented;
		}
	}
}
