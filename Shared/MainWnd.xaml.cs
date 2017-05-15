using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class MainWnd : ContentPage
	{
		public static MainWnd Current { get; set; }

		NotebooksView NotebooksView { get { return (NotebooksView)_NotebooksView.Children[0]; } }

		PageView PageView { get { return (PageView)_PageView.Children[0]; } }

		public bool IsSplitView
		{
			//get { return !(Device.Idiom == TargetIdiom.Phone || (DeviceEx.Orientation == DeviceOrientations.Portrait)); }
			get { return DeviceEx.Orientation != DeviceOrientations.Portrait; }
		}

		public MainWnd()
		{
			Current = this;
			InitializeComponent();
			NotebooksViewIsVisible = true;
		}

		//

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

		double _osa_width = -1;
		double _osa_height = -1;

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			if (this._osa_width == width && this._osa_height == height)
				return;
			this._osa_width = width;
			this._osa_height = height;

			if (!IsSplitView)
			{
				_NotebooksView.WidthRequest = width;
				_PageView.WidthRequest = width;
			}
			else
			{
				_NotebooksView.WidthRequest = Math.Max(320, width * 0.3);
				_PageView.WidthRequest = width - _NotebooksView.WidthRequest;
			}

			_NotebooksView.IsVisible = IsSplitView ? true : NotebooksViewIsVisible;
			_PageView.IsVisible = IsSplitView ? true : !NotebooksViewIsVisible;
		}

		public void View_OnSizeAllocated(double width, double height, Grid Grid, Layout<View> AppBarRow)
		{
			base.OnSizeAllocated(width, height);

			bool IsLandscape = (DeviceEx.Orientation == DeviceOrientations.Landscape);

			bool StatusBarVisible = DeviceEx.StatusBarVisible;

			Grid.RowDefinitions[0].Height =
				(IsLandscape ? Metrics.AppBarHeightLandscape : Metrics.AppBarHeightPortrait)
				+ ((StatusBarVisible) ? Metrics.StatusBarHeight : 0);

			AppBarRow.Padding = new Thickness(
				0,
				(StatusBarVisible ? Metrics.StatusBarHeight : 0),
				0,
				0);

			Grid.RowDefinitions[2].Height = (IsLandscape ? Metrics.ToolBarHeightLandscape : Metrics.ToolBarHeightPortrait);

		}
	}
}
