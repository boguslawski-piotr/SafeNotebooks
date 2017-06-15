using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pbXNet;

namespace SafeNotebooks
{
	public interface INotebooksManagerUI
	{
		Task DisplayError(NotebooksException ex);
		Task DisplayError(Exception ex);

		Task<IPassword> GetPasswordAsync(Item item, bool passwordForTheFirstTime);

		Task<(bool, IPassword)> EditItemAsync(Item item);

        // For listviews

        string LockedImageNameForLists { get; }
		double LockedImageWidthForLists { get; }

		string SelectedImageNameForLists { get; }
		string UnselectedImageNameForLists { get; }
        double SelectedUnselectedImageWidthForLists { get; }
	}
}
