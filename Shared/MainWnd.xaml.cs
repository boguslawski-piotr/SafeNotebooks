using System;
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

		public async Task DisplayError(NotebooksException ex)
		{
			string message = $"DataManagerException:Err {ex.Err}"; // TODO: wyciagac komunikaty bledow z zasobow
			await DisplayError(new Exception(message));
		}

		public async Task DisplayError(Exception ex)
		{
			await Task.Run(() =>
				Device.BeginInvokeOnMainThread(async () => await DisplayAlert(T.Localized("Error"), ex.Message, T.Localized("Cancel")))
			);
		}

		public async Task<IPassword> GetPasswordAsync(Item item, bool passwordForTheFirstTime)
		{
			// TODO: UI powinno sprawdzac poprawnosc hasla -> wykorzystac SecretsManager -> Basic authentication based on passwords
			await DisplayAlert("Password", $"is needed for: {item.Nick}; first: {passwordForTheFirstTime}", T.Localized("Cancel"));
			return new Password("123");
			//return null;
		}

		public async Task<(bool, IPassword)> EditItemAsync(Item item)
		{
			//if (!await DisplayAlert("New/Edit", $"{item.GetType().Name}", T.Localized("OK"), T.Localized("Cancel")))
			//return (false, "");

			int lll = App.Settings.GetValueOrDefault("lll", 1);

			//item.Color = Color.FromHex("#800000ff");
			item.Nick = $"{item.GetType().Name} Nick " + lll;
			item.Name = $"{item.GetType().Name} " + lll++;
			if (lll % 2 == 0)
				item.Detail = "alaal sksi dkd dkkfir fkfir fkdid dkdkf";
			//item.ThisCKeyLifeTime = CKeyLifeTime.Infinite;
			//item.ThisCKeyLifeTime = CKeyLifeTime.WhileAppRunning;
			//item.ThisCKeyLifeTime = CKeyLifeTime.OneTime;

			App.Settings.AddOrUpdateValue("lll", lll);

			return (true, new Password("123"));
		}

		//

		public string LockedImageNameForLists { get; } = "ic_lock_outline.png";
		public double LockedImageWidthForLists { get; } = Metrics.SmallIconHeight;

		public string SelectedImageNameForLists { get; } = "ic_radio_button_checked.png";
		public string UnselectedImageNameForLists { get; } = "ic_radio_button_unchecked.png";
		public double SelectedUnselectedImageWidthForLists { get; } = Metrics.SmallIconHeight;
	}

	public static class Wnd
	{
		public static MainWnd C => MainWnd.C;
	}
}
