using System;
using System.Collections.Generic;

using Xamarin.Forms;
using pbXForms;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace SafeNotebooks
{
	//[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PageWnd : ContentPageEx
	{
		MainWnd _MainWnd
		{
			get { return (MainWnd)Parent; }
		}

		public PageWnd()
		{
			InitializeComponent();

			App.Data.PageSelected += (sender, page) => ShowSelectedPage(page);

			ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null;
			//ListCtl.ItemTapped += (object sender, ItemTappedEventArgs e) => App.Data.SelectNote((Note)e.Item);

		}

		protected override void OnParentSet()
		{
			_MainWnd.NotebooksWndVisibilityChanged += NotebooksWndVisibilityChanged;

			ShowSelectedPage(App.Data.SelectedPage);
		}

		protected override void OnLayoutFixed()
		{
			NotebooksWndVisibilityChanged(this, null);
		}

		protected override void OnAppearing()
		{
		}


		public void NotebooksWndVisibilityChanged(object sender, EventArgs e)
		{
			bool NotebooksWndVisible = (bool)_MainWnd?.NotebooksWndVisible();

			BackBtn.IsVisible = !NotebooksWndVisible;

			double m = !BackBtn.IsVisible ? Metrics.ToolBarItemsWideSpacing : 0;
			SelectedPageName.Margin = new Thickness(m, 0, m, 0);
			SelectedPageParentName.Margin = new Thickness(m, 0, m, 0);
		}


		void ShowSelectedPage(Page page)
		{
			if (page != null)
			{
				AppBarRow.IsVisible = true;

				SelectedPageName.Text = page.DisplayName;
				SelectedPageParentName.Text = "in " /* TODO: translation */ + page.Parent.DisplayName;

				//EditBtn.IsVisible = true;

				ListCtl.ItemsSource = page.Notes;
				ListCtl.IsVisible = true;

				ToolBarRow.IsVisible = true;

				_MainWnd?.HideNotebooksWnd();
			}
			else
			{
				AppBarRow.IsVisible = false;

				//SelectedPageName.Text = "";
				//SelectedPageParentName.Text = "";

				//EditBtn.IsVisible = false;

				ListCtl.ItemsSource = null;
				ListCtl.IsVisible = false;

				ToolBarRow.IsVisible = false;

				_MainWnd?.ShowNotebooksWnd();
			}
		}


		void BackBtn_Clicked(object sender, System.EventArgs e)
		{
			_MainWnd?.ShowNotebooksWnd();
		}

		void EditBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Edit...", "Enable multiple items edit/delete mode?", "Cancel");
		}

		async void EditItemBtn_Clicked(object sender, System.EventArgs e)
		{
			Item item = (Item)(sender as MenuItem).CommandParameter;
			await Application.Current.MainPage.DisplayAlert("Edit", item.ToString(), "Cancel");
		}

		async void DeleteItemBtn_Clicked(object sender, System.EventArgs e)
		{
			Item item = (Item)(sender as MenuItem).CommandParameter;
			await Application.Current.MainPage.DisplayAlert("Delete", item.ToString(), "Cancel");
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

		void SortBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Sort", "Select sort for current view (ask for default?)", "Cancel");
		}


	}
}
