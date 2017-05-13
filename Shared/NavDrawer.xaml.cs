﻿using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class NavDrawer : ContentPageEx
	{
		MainFrame _frame
		{
			get { return (MainFrame)Parent; }
		}

		public NavDrawer()
		{
			InitializeComponent();

			ListCtl.ItemTapped += (object sender, ItemTappedEventArgs e) =>
			{
				if (e.Item is Notebook)
	                ShowNotebook((Notebook)e.Item);
				else
					ShowPage((Page)e.Item);
			};
	
			ListCtl.ItemSelected += (sender, e) =>
			{
				((ListView)sender).SelectedItem = null;
			};

			PageCoversStatusBar = (Device.RuntimePlatform == Device.iOS ? true : (Device.Idiom != TargetIdiom.Tablet));
		}

		protected override void OnAppearing()
		{
			ShowNotebook(App.Data.SelectedNotebook);
		}


		void SearchBtn_Clicked(object sender, System.EventArgs e)
		{
			Application.Current.MainPage.DisplayAlert("Search...", "Window for search in all data.", "Cancel");
		}

		async void SettingsBtn_Clicked(object sender, System.EventArgs e)
		{
#if __ANDROID__
			_frame.HideNavDrawer();
#endif
			await Navigation.PushModalAsync(new Settings(), true);
#if __ANDROID__
			_frame.ShowNavDrawer();
#endif
		}


		void ShowNotebooksBtn_Clicked(object sender, System.EventArgs e)
		{
			ShowNotebooks();
		}

		void ShowNotebooks()
		{
			ListCtl.ItemsSource = App.Data.Notebooks;
			SelectedNotebookName.Text = "Notebooks";    // TODO: translation
			SelectedNotebookBar.IsVisible = false;

			App.Data.SelectNotebook(null);
		}

		void ShowNotebook(Notebook notebook)
		{
			if (notebook == null)
			{
				ShowNotebooks();
			}
			else
			{
				App.Data.SelectNotebook(notebook);

				ListCtl.ItemsSource = notebook.Pages;
				SelectedNotebookName.Text = notebook.DisplayName;
				SelectedNotebookBar.IsVisible = true;
			}
		}

		void ShowPage(Page page)
		{
			App.Data.SelectPage(page);
			_frame.HideNavDrawer();
		}


		void NewBtn_Clicked(object sender, System.EventArgs e)
		{
			if (App.Data.SelectedNotebook == null)
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
					Name = "Page " + App.Data.SelectedNotebook.Pages.Count
				};

				App.Data.SelectedNotebook.AddPage(p);

				ShowPage(p);
			}
		}

	}
}
