using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class MainFrame : MasterDetailPage
	{
		public static string MsgShowNavDrawer = "MsgShowNavDrawer";
		public static string MsgHideNavDrawer = "MsgHideNavDrawer";
		public static string MsgNavDrawerVisibilityChanged = "MsgNavDrawerVisibilityChanged";

		public MainFrame()
		{
			InitializeComponent();

			IsPresentedChanged += NavDrawerVisibilityChanged;

			MessagingCenter.Subscribe<Xamarin.Forms.Page>(this, MainFrame.MsgShowNavDrawer, ShowNavDrawer);
			MessagingCenter.Subscribe<Xamarin.Forms.Page>(this, MainFrame.MsgHideNavDrawer, HideNavDrawer);
		}

		protected override void OnAppearing()
		{
		}


		void ShowNavDrawer(Xamarin.Forms.Page obj)
		{
			IsPresented = true;
		}

		void HideNavDrawer(Xamarin.Forms.Page obj)
		{
			IsPresented = false;
		}

		void NavDrawerVisibilityChanged(object sender, EventArgs e)
		{
			MessagingCenter.Send<MainFrame, bool>(this, MainFrame.MsgNavDrawerVisibilityChanged, IsPresented);
		}
	}
}
