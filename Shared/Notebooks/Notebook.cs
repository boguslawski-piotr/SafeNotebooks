using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using pbXNet;

namespace SafeNotebooks
{
	public class Notebook : ItemWithItems
	{
		public override string DetailForLists => $"{ModifiedOn.ToLocalTime().ToString()}, {Storage?.Name}";

		public const string IdForStoragePrefix = "N-";

		public override string IdForStorage => IdForStoragePrefix + base.IdForStorage;

		protected override async Task<bool> InternalLoadAsync(bool tryToUnlockChildren)
		{
			if (!await base.InternalLoadAsync(true))
				return false;

			string pattern = Page.IdForStoragePrefix + Id + "-\\w*";
			bool anyPageLoaded = await NotebooksManager.LoadItemsForItemAsync<Page>(this, pattern, tryToUnlockChildren);

			if (anyPageLoaded)
				SortItems();

			NotebooksManager.OnNotebookLoaded(this, anyPageLoaded);
			return true;
		}

		public async Task<Page> NewPageAsync()
		{
			Page page = await NotebooksManager.NewItemAsync<Page>(this);
			if (page == null)
				return null;

			AddItem(page);
			SortItems();

			return page;
		}
	}
}
