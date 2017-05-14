using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SafeNotebooks
{
	//[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NotebooksWnd : ContentPageEx
	{
		MainWnd _MainWnd
		{
			get { return (MainWnd)Parent; }
		}

		public NotebooksWnd()
		{
			InitializeComponent();

			App.Data.NotebookSelected += (sender, notebook) => ShowSelectedNotebook();

			ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null;

			ListCtl.ItemTapped += (object sender, ItemTappedEventArgs e) =>
			{
				if (e.Item is Notebook)
					App.Data.SelectNotebook((Notebook)e.Item);
				else
					App.Data.SelectPage((Page)e.Item);
			};

			PageCoversStatusBar = (Device.RuntimePlatform == Device.iOS ? true : (Device.Idiom != TargetIdiom.Tablet));

		}

		protected override void OnParentSet()
		{
			ShowSelectedNotebook();
		}

		protected override void OnAppearing()
		{
		}


		void ShowSelectedNotebook()
		{
			if (App.Data.SelectedNotebook == null)
			{
				ListCtl.ItemsSource = App.Data.Notebooks;
				SelectedNotebookName.Text = "Notebooks";    // TODO: translation
				SelectedNotebookBar.IsVisible = false;
			}
			else
			{
				ListCtl.ItemsSource = App.Data.SelectedNotebook.Pages;
				SelectedNotebookName.Text = App.Data.SelectedNotebook.DisplayName;
				SelectedNotebookBar.IsVisible = true;
			}
		}


		void SearchBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Search...", "Window for search in all data.", "Cancel");
		}

		async void SettingsBtn_Clicked(object sender, System.EventArgs e)
		{
#if __ANDROID__
			_MainWnd?.HideNotebooksWnd();
#endif
			await Navigation.PushModalAsync(new SettingsWnd(), true);
			_MainWnd?.ShowNotebooksWnd();
		}


		void BackBtn_Clicked(object sender, System.EventArgs e)
		{
			App.Data.SelectNotebook(null);
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

		void NewBtn_Clicked(object sender, System.EventArgs e)
		{
			if (App.Data.SelectedNotebook == null)
			{
				Notebook n = new Notebook()
				{
					Name = "Notebook " + App.Data.Notebooks.Count
				};

				App.Data.Notebooks.Add(n);

				App.Data.SelectNotebook(n);
			}
			else
			{
				Page p = new Page()
				{
					Name = "Page " + App.Data.SelectedNotebook.Pages.Count
				};

				App.Data.SelectedNotebook.AddPage(p);

				App.Data.SelectPage(p);
			}
		}

		void SortBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Sort", "Select sort for current view (ask for default?)", "Cancel");
		}

	}
}
