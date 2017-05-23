using System;
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

        // Images for listviews

        string LockedImageName { get; }
		string OpenImageName { get; }
	}
}
