using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SafeNotebooks
{
	//[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainWnd : MasterDetailPage
	{
		public MainWnd()
		{
			InitializeComponent();

			IsPresentedChanged += (sender, e) => NotebooksWndVisibilityChanged?.Invoke(sender, e);
		}

		public event EventHandler NotebooksWndVisibilityChanged = null;

		public bool NotebooksWndVisible()
		{
			return IsPresented;
		}

		public void ShowNotebooksWnd()
		{
			if(IsPresented != true)
				IsPresented = true;
		}

		public void HideNotebooksWnd()
		{
			if (IsPresented != false)
				if(Device.Idiom == TargetIdiom.Phone || DeviceEx.Orientation == DeviceOrientations.Portrait)
					IsPresented = false;
		}

	}
}
