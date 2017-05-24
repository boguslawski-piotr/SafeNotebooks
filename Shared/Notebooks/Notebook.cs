using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using pbXNet;
using pbXSecurity;

namespace SafeNotebooks
{
    public class ItemWChildren<T> : Item where T: Item
    {
        protected IList<T> _Items = new List<T>();
		public ObservableCollection<T> Items { get; protected set; } = new ObservableCollection<T>();

        public void AddItem(T item)
        {
            _Items.Add(item);
        }

		public void Sort()
		{
			// TODO: sortowanie zapamietaniem wyboru
			Items = new ObservableCollection<T>(_Items.OrderBy((v) => v.NameForLists));

            NotebooksManager.OnItemItemsSorted(this);
            //NotebooksSorted?.Invoke(this, this);
		}

	}

    public class Notebook : ItemWChildren<Page>
    {
        public async Task<Page> NewPageAsync()
        {
            Page page = new Page() { NotebooksManager = NotebooksManager };
            if (!await NotebooksManager.NewItemHelperAsync(page, this))
                return null;

            AddItem(page);
            Sort();

            return page;
        }

        public async Task AddPageAsync(Page page)
        {
            await page.ChangeParentAsync(this);

            AddItem(page);
			Sort();
		}

        //

        public const string IdForStoragePrefix = "N-";

        public override string IdForStorage => IdForStoragePrefix + base.IdForStorage;

        public override async Task<bool> LoadAsync(bool tryToUnlockChildren)
        {
            if (!await base.LoadAsync(true))
                return false;

            string pattern = Page.IdForStoragePrefix + Id + "-\\w*";
            bool anyPageLoaded = await NotebooksManager.LoadChildrenForItemHelperAsync<Page>(this, pattern, tryToUnlockChildren);

			if (anyPageLoaded)
				Sort();

            NotebooksManager.OnNotebookLoaded(this, anyPageLoaded);
            return true;
        }


        //

        public override string DetailForLists => $"{Storage?.Name}, {ModifiedOn.ToLocalTime().ToString()}";

    }
}
