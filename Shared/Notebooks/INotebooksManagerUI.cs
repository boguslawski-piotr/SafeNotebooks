﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pbXNet;

namespace SafeNotebooks
{
	public interface INotebooksManagerUI
	{
		Task DisplayError(NotebooksException.ErrorCode err);
		Task DisplayError(string message);

		Task<string> GetPasswordAsync(Item item, bool passwordForTheFirstTime);

        Task<(bool, string)> EditItemAsync(Item item);

        // For listviews

        string LockedImageNameForLists { get; }
		double LockedImageWidthForLists { get; }

		string SelectedImageNameForLists { get; }
		string UnselectedImageNameForLists { get; }
        double SelectedUnselectedImageWidthForLists { get; }
	}
}
