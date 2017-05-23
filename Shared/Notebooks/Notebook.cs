using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using pbXNet;
using pbXSecurity;

namespace SafeNotebooks
{
    public class Notebook : Item
    {
        public ObservableCollection<Page> Pages { get; private set; } = new ObservableCollection<Page>();

        public async Task<Page> NewPageAsync()
        {
            Page page = new Page() { NotebooksManager = NotebooksManager };
            if (!await NotebooksManager.NewItemHelperAsync(page, this))
                return null;

            Pages.Add(page);
            // sort pages

            return page;
        }

        public async Task AddPageAsync(Page page)
        {
            await page.ChangeParentAsync(this);
            Pages.Add(page);
        }

        //

        public const string IdForStoragePrefix = "N-";

        public override string IdForStorage => IdForStoragePrefix + base.IdForStorage;

        public override async Task<bool> LoadAsync(bool tryToUnlockChildren)
        {
            if (!await base.LoadAsync(true))
                return false;

            string pattern = Page.IdForStoragePrefix + Id + "-\\w*";
            await NotebooksManager.LoadChildrenForItemHelperAsync<Page>(this, Pages, pattern, tryToUnlockChildren);

            NotebooksManager.NotebookLoadedAsync(this);
            return true;
        }


        //

        public override string DetailForLists => $"{Storage?.Name}, {ModifiedOn.ToLocalTime().ToString()}";

    }
}
