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

		public ListOfNotes()
		{
			InitializeComponent();

			App.Data.PageSelected += (sender, page) =>
			{
                ShowPage(page);
			};

			ListCtl.ItemTapped += (sender, e) =>
			{
			};

			ListCtl.ItemSelected += (sender, e) => 
			{
    			((ListView)sender).SelectedItem = null;
			};

		}

		protected override void OnParentSet()
		{
			_frame.IsPresentedChanged += NavDrawerVisibilityChanged;
		}

		protected override void OnAppearing()
		{
			ShowPage(App.Data.SelectedPage);
		}

		protected override void OnLayoutFixed()
		{
			NavDrawerVisibilityChanged(this, null);
		}


		void NavDrawerBtn_Clicked(object sender, System.EventArgs e)
		{
			_frame.ShowNavDrawer();
		}

		public void NavDrawerVisibilityChanged(object sender, EventArgs e)
		{
			bool NavDrawerVisible = _frame.IsPresented;

			NavDrawerBtn.IsVisible = !NavDrawerVisible;

						double m = !NavDrawerBtn.IsVisible ? Metrics.ToolBarItemsWideSpacing : 0;
			SelectedPageName.Margin = new Thickness(m, 0, m, 0);
			SelectedPageParentName.Margin = new Thickness(m, 0, m, 0);
		}


		void PageSelected(object sender, SafeNotebooks.Page page)
		{
            ShowPage(page);
		}

		void ShowPage(Page page) 
		{
			if (page != null)
			{
				SelectedPageName.Text = page.DisplayName;
				SelectedPageParentName.Text = "in " /* TODO: translation */ + page.Parent.DisplayName;

				EditBtn.IsVisible = true;

				ListCtl.ItemsSource = page.Notes;
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

				_frame.ShowNavDrawer();
			}
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

			App.Data.SelectedPage.AddNote(note);
		}
	}
}
