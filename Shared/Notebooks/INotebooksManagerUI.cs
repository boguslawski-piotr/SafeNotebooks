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

		Task<IFileSystem> SelectFileSystemAsync(IEnumerable<IFileSystem> FileSystems);

        Task<(bool, string)> EditItemAsync(Item item);

        // Images for list views

        string LockedImageName { get; }
		string OpenImageName { get; }
	}
}
