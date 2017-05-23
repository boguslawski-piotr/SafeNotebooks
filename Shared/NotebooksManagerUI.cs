﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using pbXNet;
using pbXSecurity;

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
			await App.Current.MainPage.DisplayAlert("Password", $"is needed for: {item.Nick}; first: {passwordForTheFirstTime}", T.Localized("Cancel"));
            return "123";
            //return null;
        }


        static int xxx = 0;

        public async Task<(bool, string)> EditItemAsync(Item item)
        {
            await App.Current.MainPage.DisplayAlert("New/Edit", $"{item.GetType().Name}", T.Localized("OK"), T.Localized("Cancel"));
			
			item.Nick = $"{item.GetType().Name} Nick " + xxx++;
            item.Name = $"{item.GetType().Name} " + xxx++;
			//item.ThisCKeyLifeTime = CKeyLifeTime.Infinite;
			item.ThisCKeyLifeTime = CKeyLifeTime.WhileAppRunning;

            return (true, "123");
        }


        //

        public string LockedImageName { get; } = "ic_lock_outline.png";
        public string OpenImageName { get; } = "ic_chevron_right.png";

	}
}