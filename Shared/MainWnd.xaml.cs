using System;
using System.Collections.Generic;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class MainWnd : ContentPage
	{
		public static MainWnd Current { get; set; }

		NotebooksView NotebooksView { get { return (NotebooksView)_NotebooksView.Children[0]; } }

		PageView PageView { get { return (PageView)_PageView.Children[0]; } }

		public MainWnd()
		{
			Current = this;
			InitializeComponent();
			NotebooksViewIsVisible = true;
		}

		//

		public bool IsSplitView
		{
			//get { return !(Device.Idiom == TargetIdiom.Phone || (DeviceEx.Orientation == DeviceOrientations.Portrait)); }
			get { return DeviceEx.Orientation != DeviceOrientations.Portrait || Device.Idiom != TargetIdiom.Phone; }
		}

		bool _NotebooksViewIsVisible;
		public bool NotebooksViewIsVisible
		{
			get {
				return _NotebooksViewIsVisible;
			}
			set {
				_NotebooksViewIsVisible = !IsSplitView ? value : true;
				_NotebooksView.IsVisible = _NotebooksViewIsVisible;
				_PageView.IsVisible = IsSplitView ? true : !_NotebooksViewIsVisible;
			}
		}

		//

		Size _osa;

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			if (!Tools.IsDifferent(new Size(width, height), ref _osa))
				return;
            
			if (!IsSplitView)
			{
				_NotebooksView.WidthRequest = width;
				_PageView.WidthRequest = width;
			}
			else
			{
				_NotebooksView.WidthRequest = Math.Max(240, width * 0.3);
				_PageView.WidthRequest = width - _NotebooksView.WidthRequest;
			}

			_NotebooksView.IsVisible = IsSplitView ? true : NotebooksViewIsVisible;
			_PageView.IsVisible = IsSplitView ? true : !NotebooksViewIsVisible;
		}
	}
}
