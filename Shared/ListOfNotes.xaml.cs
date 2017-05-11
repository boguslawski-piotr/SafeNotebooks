using System;
using System.Collections.Generic;

using Xamarin.Forms;
using pbXForms;
using System.Threading.Tasks;

namespace SafeNotebooks
{
	public partial class ListOfNotes : ContentPageWAppBar
	{
		List<string> l;

		public ListOfNotes()
		{
			InitializeComponent();


			//ListCtl.IsPullToRefreshEnabled = true;
			//ListCtl.Refreshing += ListCtl_Refreshing; ;
			l = new List<string>()
			{
			  "mono",
			  "monodroid",
			  "monotouch",
			  "monorail",
			  "monodevelop",
			  "monotone",
			  "monopoly",
			  "monomodal",
			  "mononucleosis",
			  "2mono",
			  "2monodroid",
			  "2monotouch",
			  "2monorail",
			  "2monodevelop",
			  "2monotone",
			  "2monopoly",
			  "2monomodal",
			  "2mononucleosis",
			};
			ListCtl.ItemsSource = l;

			MessagingCenter.Subscribe<MainFrame, bool>(this, App.MsgNavDrawerVisibilityChanged, NavDrawerVisibilityChanged);

		}

		protected override void OnAppearing()
		{
			//if (l.Count > 0)
			//	ListCtl.ScrollTo(l[0], ScrollToPosition.Start, false);
			//else
			//{
			//	(ListCtl.Header as StackLayout).IsVisible = false;
			//}
		}


		protected override void AdjustAppBar(bool IsLandscape)
		{
#if __IOS__
			bool StatusBarVisible = !IsLandscape || Device.Idiom == TargetIdiom.Tablet;
#endif
#if __ANDROID__
			bool StatusBarVisible = false;
#endif
			AppBar.HeightRequest = (IsLandscape? Metrics.AppBarHeightLandscape : Metrics.AppBarHeightPortrait);
			AppBar.Padding = new Thickness(
				Metrics.ScreenEdgeLeftRightMargin,
				StatusBarVisible? Metrics.StatusBarHeight : 0,
				Metrics.ScreenEdgeLeftRightMargin,
				0);
		}

		protected override void AdjustToolBar(bool IsLandscape)
		{
			ToolBar.HeightRequest = (IsLandscape ? Metrics.ToolBarHeightLandscape : Metrics.ToolBarHeightPortrait);
			ToolBar.Padding = new Thickness(
				Metrics.ScreenEdgeLeftRightMargin,
				0,
				Metrics.ScreenEdgeLeftRightMargin,
				0);
		}


		public void NavDrawerVisibilityChanged(MainFrame MainFrame, bool IsVisible)
		{
		#if __IOS__
					NavDrawerBtn.IsVisible = !IsVisible;
		#endif
		}

		void NavDrawerBtn_Clicked(object sender, System.EventArgs e)
		{
			MessagingCenter.Send<Page>(this, App.MsgChangeNavDrawerVisibility);
		}


		void ListCtl_Refreshing(object sender, EventArgs e)
		{
			ListCtl.EndRefresh();
		}


		async void NewBtn_Clicked(object sender, System.EventArgs e)
		{
			string rc = await Application.Current.MainPage.DisplayActionSheet("New...", "Cancel", null, "Note", "Task", "Account", "Identity");
			New(rc);
		}

		void FavoriteNewBtn_Clicked(object sender, System.EventArgs e)
		{
			New("Note");
		}

		void New(string what)
		{
			Application.Current.MainPage.DisplayAlert("Create new", what, "Cancel");
		}
	}
}
