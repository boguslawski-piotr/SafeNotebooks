using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class NotebooksView : BaseView
	{
		public NotebooksView()
		{
			InitializeComponent();

			Wnd.C.MasterViewWillBeShown += MasterViewWillBeShown;

			App.C.NotebooksManager.NotebooksAreStartingToLoad += NotebooksAreStartingToLoad;
			App.C.NotebooksManager.NotebooksLoaded += NotebooksLoaded;

			App.C.NotebooksManager.ItemObservableItemsCreated += ObservableItemsCreated;

			ListCtl.ItemSelected += (sender, e) => ((ListView)sender).SelectedItem = null; // disable item selection
			ListCtl.ItemTapped += ListCtl_ItemTapped;

			ListCtl.IsPullToRefreshEnabled = true;
			ListCtl.RefreshCommand = new Command(RefreshNotebooks);

			InitializeSearchBarFor(ListCtl);
		}

		protected override void ContinueOnSizeAllocated(double width, double height)
		{
			base.ContinueOnSizeAllocated(width, height);

			if (Wnd.C.IsSplitView)
				AppTitle.FontSize = Device.GetNamedSize(NamedSize.Medium, AppTitle);
			else
				AppTitle.FontSize = Device.GetNamedSize(NamedSize.Large, AppTitle);
		}

		//

		void MasterViewWillBeShown(object sender, (View view, object param) e)
		{
			if (e.view == this)
				Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollTo(ListCtl, App.C.NotebooksManager.SelectedNotebook));
		}

		void NotebooksAreStartingToLoad(object sender, EventArgs ea)
		{
			Device.BeginInvokeOnMainThread(() => AIIsVisible(true));
		}

		void ObservableItemsCreated(object sender, ItemWithItems forWhom)
		{
			if (forWhom is NotebooksManager)
				Device.BeginInvokeOnMainThread(() => ListCtl.ItemsSource = forWhom.ObservableItems);
		}

		void NotebooksLoaded(object sender, bool anyNotebookLoaded)
		{
			Device.BeginInvokeOnMainThread(() => AIIsVisible(false));
		}

		void AIIsVisible(bool isVisible)
		{
			AI.IsVisible = isVisible;
			AI.IsRunning = isVisible;
		}


		//

		async Task RefreshNotebooksAsync()
		{
			await App.C.NotebooksManager.LoadNotebooksAsync(App.C.StoragesManager.Storages, App.Settings.TryToUnlockItemItems);
			Device.BeginInvokeOnMainThread(ListCtl.EndRefresh);
		}

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

		void RefreshNotebooks()
		{
			RefreshNotebooksAsync();
		}

#pragma warning restore CS4014

		async Task SelectNotebook(Notebook notebook)
		{
			await Wnd.C.ShowMasterViewAsync<NotebookView>(MastersDetailsPage.ViewsSwitchingAnimation.Forward, notebook);
			await App.C.NotebooksManager.SelectNotebookAsync(notebook, App.Settings.TryToUnlockItemItems);
		}

		async void ListCtl_ItemTapped(object sender, ItemTappedEventArgs e)
		{
			if (e.Item is Notebook notebook)
			{
				if (App.C.NotebooksManager.SelectModeForItemsEnabled)
					notebook.IsSelected = !notebook.IsSelected;
				else
				{
					await SelectNotebook(notebook);
				}
			}
		}


		//

		SettingsDlg dlg;

		async void SettingsBtn_Clicked(object sender, System.EventArgs e)
		{
			if (dlg == null)
				dlg = new SettingsDlg();

			dlg.MinimumHeightRequest = Bounds.Height - 3 * Metrics.ScreenEdgeMargin;
			dlg.InitializeUI();
			//dlg.MaximumHeightRequest = 720;

			await Wnd.C.ModalManager.DisplayModalAsync(dlg, Device.Idiom != TargetIdiom.Phone ? ModalViewsManager.ModalPosition.Center : ModalViewsManager.ModalPosition.WholeView);
		}


		//

		public override void SearchQueryChanged(string text)
		{
		}


		//

		void SortBtn_Clicked(object sender, System.EventArgs e)
		{
			//string[] cs = { "#ffff0000", "#ff008000", "#80ff0000", "#80008000", "#40ff0000", "#40008000", };
			//foreach (var c in cs)
			//{
			//    Notebook n = await App.NotebooksManager.NewNotebookAsync(App.StoragesManager.Storages.First());
			//    n.Color = Color.FromHex(c);
			//    //await n.SaveAsync();
			//}
			//App.NotebooksManager.SortNotebooks();

			base.SortBtn_Clicked(T.Localized("Notebooks"), App.C.NotebooksManager, ListCtl);
		}

		async Task<StorageOnFileSystem<string>> SelectStorageUIAsync(IEnumerable<StorageOnFileSystem<string>> storages)
		{
			// TODO: do zmiany na wlasny dialog (modal view)
			string fsName = await App.Current.MainPage.DisplayActionSheet(T.Localized("WhereStoreNotebook"),
																		  T.Localized("Cancel"),
																		  null,
																		  storages.Select((storage1) => storage1.Name).ToArray());
			try
			{
				return storages.First((storage2) => storage2.Name == fsName);
			}
			catch
			{
				return null;
			}
		}

		async void NewBtn_Clicked(object sender, System.EventArgs e)
		{
			StorageOnFileSystem<string> storage = await App.C.StoragesManager.SelectStorageAsync(SelectStorageUIAsync);
			if (storage != null)
			{
				//for (int i = 0; i < 200; i++)
				//{
				//    await App.NotebooksManager.NewNotebookAsync(storage);
				//}
				Notebook notebook = await App.C.NotebooksManager.NewNotebookAsync(storage);
				if (notebook != null)
				{
					Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollTo(ListCtl, notebook));
					await SelectNotebook(notebook);
				}
			}
		}

		void DeleteSelectedItemsBtn_Clicked(object sender, System.EventArgs e)
		{
		}

		void MoveSelectedItemsBtn_Clicked(object sender, System.EventArgs e)
		{
		}

		async void ToogleSelectModeBtn_Clicked(object sender, System.EventArgs e)
		{
			MoveSelectedItemsBtn.IsEnabled = App.C.StoragesManager.Storages.Count() > 1;
			App.C.NotebooksManager.SelectModeForItemsEnabled = !App.C.NotebooksManager.SelectModeForItemsEnabled;

			await Task.Delay((int)(DeviceEx.AnimationsLength * 1.33));

			SelectModeToolbar.IsVisible = App.C.NotebooksManager.SelectModeForItemsEnabled;
			NormalToolbar.IsVisible = !App.C.NotebooksManager.SelectModeForItemsEnabled;
		}
	}
}
