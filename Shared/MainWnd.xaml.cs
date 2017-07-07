using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class MainWnd : MastersDetailsPage, INotebooksManagerUI
	{
		public static MainWnd Current { get; set; }
		public static MainWnd C => Current;

		public MainWnd()
		{
			Current = this;

			InitializeComponent();
			InitializeViews();
		}

		//

		public void BeginInvokeOnMainThread(Action action)
		{
			Device.BeginInvokeOnMainThread(action);
		}

		public async Task DisplayError(NotebooksException ex, object caller = null, [CallerMemberName]string callerName = null)
		{
			string message = $"NotebooksException:Err {ex.Err}"; // TODO: wyciagac komunikaty bledow z zasobow
			await DisplayError(new Exception(message), caller, callerName);
		}

		public async Task DisplayError(Exception ex, object caller = null, [CallerMemberName]string callerName = null)
		{
			string message = Log.E(ex, caller, callerName);
			await Task.Run(() =>
				BeginInvokeOnMainThread(async () => await DisplayAlert(T.Localized("Error"), message, T.Localized("OK")))
			);
		}

		public async Task<IPassword> GetPasswordAsync(Item item, bool passwordForTheFirstTime)
		{
			await DisplayAlert("Password", $"is needed for: {item.Nick}; first: {passwordForTheFirstTime}", T.Localized("Cancel"));
			return new Password("123");
			//return null;
		}

		static int lll = 0;

		public async Task<(bool, IPassword)> EditItemAsync(Item item)
		{
			//if (!await DisplayAlert("New/Edit", $"{item.GetType().Name}", T.Localized("OK"), T.Localized("Cancel")))
			//return (false, "");


			//item.Color = "#800000ff";
			item.Nick = $"{item.GetType().Name} Nick " + lll;
			item.Name = $"{item.GetType().Name} " + lll++;
			if (lll % 2 == 0)
				item.Detail = "alaal sksi dkd dkkfir fkfir fkdid dkdkf";
			//item.ThisCKeyLifeTime = CKeyLifeTime.Infinite;
			//item.ThisCKeyLifeTime = SecretLifeTime.WhileAppRunning;
			//item.ThisCKeyLifeTime = CKeyLifeTime.OneTime;

			return (true, new Password("123"));
		}

		//

		public string LockedImageForListsName { get; } = "ic_lock_outline.png";
		public double LockedImageForListsWidth { get; } = Metrics.SmallIconHeight;

		public string SelectedImageForListsName { get; } = "ic_radio_button_checked.png";
		public string UnselectedImageForListsName { get; } = "ic_radio_button_unchecked.png";
		public double SelectedUnselectedImageForListsWidth { get; } = Metrics.SmallIconHeight;
	}

	public static class Wnd
	{
		public static MainWnd C => MainWnd.C;
	}
}
