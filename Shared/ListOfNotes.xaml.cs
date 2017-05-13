using System;
using System.Collections.Generic;

using Xamarin.Forms;
using pbXForms;
using System.Threading.Tasks;

namespace SafeNotebooks
{
	public partial class ListOfNotes : ContentPageEx
	{
		MainFrame _frame
		{
			get { return (MainFrame)Parent; }
		}

		Page SelectedPage = null;

		public ListOfNotes()
		{
			InitializeComponent();

			ListCtl.ItemTapped += ListCtl_ItemTapped;
			ListCtl.ItemSelected += (sender, e) => {
    			((ListView)sender).SelectedItem = null;
			};

			MessagingCenter.Subscribe<Xamarin.Forms.Page, Page>(this, MainFrame.MsgPageSelected, PageSelected);
			//MessagingCenter.Subscribe<MainFrame, bool>(this, MainFrame.MsgNavDrawerVisibilityChanged, NavDrawerVisibilityChanged);
		}

		protected override void OnParentSet()
		{
			_frame.IsPresentedChanged += NavDrawerVisibilityChanged;
		}

		protected override void OnAppearing()
		{
			ShowPage(SelectedPage);
		}

		protected override void OnLayoutFixed()
		{
			NavDrawerVisibilityChanged(this, null);
		}


		void NavDrawerBtn_Clicked(object sender, System.EventArgs e)
		{
			MessagingCenter.Send<Xamarin.Forms.Page>(this, MainFrame.MsgShowNavDrawer);
		}

		public void NavDrawerVisibilityChanged(object sender, EventArgs e)
		{
			bool NavDrawerVisible = _frame.IsPresented;

			NavDrawerBtn.IsVisible = !NavDrawerVisible;

						double m = !NavDrawerBtn.IsVisible ? Metrics.ToolBarItemsWideSpacing : 0;
			SelectedPageName.Margin = new Thickness(m, 0, m, 0);
			SelectedPageParentName.Margin = new Thickness(m, 0, m, 0);
		}


		void PageSelected(Xamarin.Forms.Page who, Page page)
		{
			ShowPage(page);
		}

		void ShowPage(Page page) 
		{
			SelectedPage = page;
			if (SelectedPage != null)
			{
				SelectedPageName.Text = SelectedPage.DisplayName;
				SelectedPageParentName.Text = "in " /* TODO: translation */ + SelectedPage.Parent.DisplayName;

				EditBtn.IsVisible = true;

				ListCtl.ItemsSource = SelectedPage.Notes;
				ListCtl.IsVisible = true;

				ToolBarRow.IsVisible = true;
			}
			else
			{
				SelectedPageName.Text = "";
				SelectedPageParentName.Text = "";

				EditBtn.IsVisible = false;

				ListCtl.ItemsSource = null;
				ListCtl.IsVisible = false;

				ToolBarRow.IsVisible = false;

				MessagingCenter.Send<Xamarin.Forms.Page>(this, MainFrame.MsgShowNavDrawer);
			}
		}


		void ListCtl_Refreshing(object sender, EventArgs e)
		{
			ListCtl.EndRefresh();
		}

		void ListCtl_ItemTapped(object sender, ItemTappedEventArgs e)
		{
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
			//Application.Current.MainPage.DisplayAlert("Create new", what, "Cancel");
			Note note = new Note()
			{
				Name = DateTime.Now.ToString()
			};

			SelectedPage.AddNote(note);
		}
	}
}
