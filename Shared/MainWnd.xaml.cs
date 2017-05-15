using System;
using pbXForms;
using Xamarin.Forms;

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

		//protected override void OnSizeAllocated(double width, double height)
		//{
		//	base.OnSizeAllocated(width, height);
		//	MasterBounds = new Rectangle(0, 0, Bounds.Width, Bounds.Height);
		//}

		public event EventHandler NotebooksWndVisibilityChanged = null;

		public bool NotebooksWndVisible()
		{
			return IsPresented;
		}

		public void ShowNotebooksWnd()
		{
			if (IsPresented != true)
				IsPresented = true;
		}

		public void HideNotebooksWnd()
		{
			if (IsPresented != false)
				if (Device.Idiom == TargetIdiom.Phone || DeviceEx.Orientation == DeviceOrientations.Portrait)
					IsPresented = false;
		}

	}
}
