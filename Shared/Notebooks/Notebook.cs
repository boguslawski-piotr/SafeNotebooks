using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using pbXNet;
using pbXSecurity;

namespace SafeNotebooks
{
    public class Notebook : ItemWithItems<Page>
    {
        public override string DetailForLists => $"{ModifiedOn.ToLocalTime().ToString()}, {Storage?.Name}";

        public const string IdForStoragePrefix = "N-";

        public override string IdForStorage => IdForStoragePrefix + base.IdForStorage;

        protected override async Task<bool> InternalLoadAsync(bool tryToUnlockChildren)
        {
            if (!await base.InternalLoadAsync(true))
                return false;

            string pattern = Page.IdForStoragePrefix + Id + "-\\w*";
            bool anyPageLoaded = await NotebooksManager.LoadChildrenForItemHelperAsync<Page>(this, pattern, tryToUnlockChildren);

            if (anyPageLoaded)
                SortItems();

            NotebooksManager.OnNotebookLoaded(this, anyPageLoaded);
            return true;
        }

        public async Task<Page> NewPageAsync()
        {
            Page page = new Page() { NotebooksManager = NotebooksManager };
            if (!await NotebooksManager.NewItemHelperAsync(page, this))
                return null;

            AddItem(page);
            SortItems();

            return page;
        }

        public async Task AddPageAsync(Page page)
        {
            await page.ChangeParentAsync(this);

            AddItem(page);
            SortItems();
        }

        public override void SortItems()
        {
            base.SortItems();
            NotebooksManager.OnPagesSorted(this);
        }
    }
}
