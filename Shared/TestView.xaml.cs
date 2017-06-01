using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class TestView : ModalContentView
    {
        public TestView()
        {
            InitializeComponent();
        }

        void Hide_Clicked(object sender, System.EventArgs e)
        {
            //MainWnd.Current.IsSplitView = true;
            //MainWnd.Current.ShowDetailView<PageView>("vPage", MasterDetailPageEx.ViewsSwitchingAnimation.LeftToRight);
			MainWnd.Current.ShowDetailViewAsync<PageView>(MasterDetailPageEx.ViewsSwitchingAnimation.Back);
		}
	
        void MS_Clicked(object sender, System.EventArgs e)
		{
            MainWnd.Current.ShowMasterViewAsync<NotebookView>( MasterDetailPageEx.ViewsSwitchingAnimation.Back);
            MainWnd.Current.MasterViewIsVisible = true;
		}

        void MH_Clicked(object sender, System.EventArgs e)
		{
            MainWnd.Current.MasterViewIsVisible = false;
		}
	}
}
