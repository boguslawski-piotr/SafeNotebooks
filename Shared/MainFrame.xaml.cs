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
#if __IOS__
			if (IsPresented)
				Master.Title = "[current notebook]";
			else
				Master.Title = "[ ]";
#endif
#if __ANDROID__
			if (IsPresented)
				Master.Title = "[app name?]";
			else
				Master.Title = "";
#endif

		}

	}
}
