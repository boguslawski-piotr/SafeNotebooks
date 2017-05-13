using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class MainFrame : MasterDetailPage
	{

		public static string MsgPageSelected = "MsgPageSelected";

		public MainFrame()
		{
			InitializeComponent();
		}


		public void ShowNavDrawer()
		{
			IsPresented = true;
		}

		public void HideNavDrawer()
		{
			if(Device.Idiom == TargetIdiom.Phone || DeviceEx.Orientation == DeviceOrientations.Portrait)
				IsPresented = false;
		}

	}
}
