using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class NavDrawer : ContentPageEx
	{
		public Notebook SelectedNotebook = null;

		public NavDrawer()
		{
			InitializeComponent();

			ListCtl.ItemTapped += ListCtl_ItemTapped;
			ListCtl.ItemSelected += (sender, e) => {
    			((ListView)sender).SelectedItem = null;
			};

			AppBarCoversStatusBar = (Device.RuntimePlatform == Device.iOS ? true : (Device.Idiom != TargetIdiom.Tablet));
		}

		protected override void OnAppearing()
		{
			ShowNotebook(SelectedNotebook);
		}


		void SearchBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Search...", "Window for search in all data.", "Cancel");
		}

		async void SettingsBtn_Clicked(object sender, System.EventArgs e)
		{
#if __ANDROID__
			MessagingCenter.Send<Xamarin.Forms.Page>(this, MainFrame.MsgHideNavDrawer);
#endif
			await Navigation.PushModalAsync(new Settings(), true);
#if __ANDROID__
			MessagingCenter.Send<Xamarin.Forms.Page>(this, MainFrame.MsgShowNavDrawer);
#endif
		}


		void ShowNotebooksBtn_Clicked(object sender, System.EventArgs e)
		{
			ShowNotebooks();
		}

		void ShowNotebooks()
		{
			ListCtl.ItemsSource = App.Data.Notebooks;
			SelectedNotebookName.Text = "Notebooks";	// TODO: translation
			SelectedNotebookBar.IsVisible = false;

			SelectedNotebook = null;
		}


		void ListCtl_ItemTapped(object sender, ItemTappedEventArgs e)
		{
			if (e.Item is Notebook)
			{
				ShowNotebook((Notebook)e.Item);
			}
			else
			{
				ShowPage((Page)e.Item);
			}
		}

		void ShowNotebook(Notebook n)
		{
			SelectedNotebook = n;

			if (SelectedNotebook == null) 
			{
				ShowNotebooks();
			}
			else
			{
				ListCtl.ItemsSource = SelectedNotebook.Pages;
				SelectedNotebookName.Text = SelectedNotebook.Name;
				SelectedNotebookBar.IsVisible = true;
			}
		}

		void ShowPage(Page p)
		{
			Page SelectedPage = p;

			MessagingCenter.Send<Xamarin.Forms.Page, Page>(this, MainFrame.MsgPageSelected, SelectedPage);
			MessagingCenter.Send<Xamarin.Forms.Page>(this, MainFrame.MsgHideNavDrawer);
		}


		void NewBtn_Clicked(object sender, System.EventArgs e)
		{
			if (SelectedNotebook == null)
			{
				Notebook n = new Notebook()
				{
					Name = "Notebook " + App.Data.Notebooks.Count
				};

				//await Application.Current.MainPage.DisplayAlert("New notebook", "Window for creating new notebook.", "Cancel");

				App.Data.Notebooks.Add(n);

				ShowNotebook(n);
			}
			else
			{
				Page p = new Page()
				{
					Name = "Page " + SelectedNotebook.Pages.Count
				};

				SelectedNotebook.AddPage(p);

				ShowPage(p);
			}
		}

	}
}
