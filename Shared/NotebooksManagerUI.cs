using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using pbXNet;
using pbXSecurity;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public class NotebooksManagerUI : INotebooksManagerUI
    {
        public async Task DisplayError(NotebooksException.ErrorCode err)
        {
            string message = $"DataManagerException:Err {err}"; // TODO: wyciagac komunikaty bledow z zasobow
            await DisplayError(message);
        }

        public async Task DisplayError(string message)
        {
			await App.Current.MainPage.DisplayAlert("Error", message, T.Localized("Cancel"));
		}
		

        public async Task<string> GetPasswordAsync(Item item, bool passwordForTheFirstTime)
        {
			// TODO: UI powinno sprawdzac poprawnosc hasla -> wykorzystac SecretsManager -> Basic authentication based on passwords
			//await App.Current.MainPage.DisplayAlert("Password", $"is needed for: {item.Nick}; first: {passwordForTheFirstTime}", T.Localized("Cancel"));
            return "123";
            //return null;
        }


        public async Task<(bool, string)> EditItemAsync(Item item)
        {
            //await App.Current.MainPage.DisplayAlert("New/Edit", $"{item.GetType().Name}", T.Localized("OK"), T.Localized("Cancel"));

            int lll = App.Settings.Current.GetValueOrDefault("lll", 1);

            //item.Color = Color.FromHex("#800000ff");
            item.Nick = $"{item.GetType().Name} Nick " + lll;
            item.Name = $"{item.GetType().Name} " + lll++;
            if (lll % 2 == 0)
                item.Detail = "alaal sksi dkd dkkfir fkfir fkdid dkdkf";
            //item.ThisCKeyLifeTime = CKeyLifeTime.Infinite;
            //item.ThisCKeyLifeTime = CKeyLifeTime.WhileAppRunning;
            //item.ThisCKeyLifeTime = CKeyLifeTime.OneTime;

			App.Settings.Current.AddOrUpdateValue("lll", lll);

            return (true, "123");
        }


        //

        public string LockedImageNameForLists { get; } = "ic_lock_outline.png";
        public double LockedImageWidthForLists { get; } = Metrics.SmallIconHeight;

        public string SelectedImageNameForLists { get; } = "ic_radio_button_checked.png";
		public string UnselectedImageNameForLists { get; } = "ic_radio_button_unchecked.png";
        public double SelectedUnselectedImageWidthForLists { get; } = Metrics.SmallIconHeight;
	}
}
