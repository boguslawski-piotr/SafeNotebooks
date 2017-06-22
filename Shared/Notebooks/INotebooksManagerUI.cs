﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pbXNet;

namespace SafeNotebooks
{
	public interface INotebooksManagerUI
	{
		void BeginInvokeOnMainThread(Action action);

		Task DisplayError(NotebooksException ex);
		Task DisplayError(Exception ex);

		Task<IPassword> GetPasswordAsync(Item item, bool passwordForTheFirstTime);

		Task<(bool, IPassword)> EditItemAsync(Item item);

        // For listviews

        string LockedImageForListsName { get; }
		double LockedImageForListsWidth { get; }

		string SelectedImageForListsName { get; }
		string UnselectedImageForListsName { get; }
        double SelectedUnselectedImageForListsWidth { get; }
	}
}
