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

			IsPresentedChanged += MainFrame_IsPresentedChanged;
			MainFrame_IsPresentedChanged(this, null);
		}

		void MainFrame_IsPresentedChanged(object sender, EventArgs e)
		{
			MessagingCenter.Send<MainFrame>(this, "NavDrawerIsPresentedChanged");
#if __IOS__
			if (IsPresented)
				Master.Title = "";
			else
				Master.Title = "\ud83d\udcd5";
#endif
#if __ANDROID__
#endif

		}

	}
}
