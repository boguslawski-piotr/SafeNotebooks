using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using pbXNet;
using pbXSecurity;

namespace SafeNotebooks
{
    public class NotebooksManager : ItemWithItems<Notebook>
    {
        public ISecretsManager SecretsManager { get; set; }
        public INotebooksManagerUI UI { get; set; }

        public NotebooksManager()
        {
            NewAsync(null);
        }


        //

		public event EventHandler<NotebooksManager> NotebooksLoadingBegun;
		public event EventHandler<NotebooksManager> NotebooksLoaded;

		public async Task LoadNotebooksAsync(IEnumerable<ISearchableStorage<string>> storages, bool tryToUnlock)
        {
            // Prepare

            await SelectNotebookAsync(null, false);

            Items.Clear();
            if(ObservableItems != null)
                ObservableItems.Clear();

            NotebooksLoadingBegun.Invoke(this, this);

            // Load notebooks from all available storages

            string pattern = Notebook.IdForStoragePrefix + "\\w*";
            foreach (var storage in storages)
            {
				IEnumerable<string> notebookIds = await storage.FindIdsAsync(pattern);
                foreach (var id in notebookIds)
                {
                    Notebook notebook = new Notebook() { NotebooksManager = this, Storage = storage };
                    await notebook.OpenAsync(null, id, tryToUnlock); 

                    AddItem(notebook);
                }
            }

            if(Items.Count > 0)
                SortItems();
            
            NotebooksLoaded?.Invoke(this, this);
        }

		
        public override void SortItems()
        {
			// TODO: sortowanie notebooks, pages, notes z zapamietaniem wyboru
            base.SortItems();

            OnNotebooksSorted();
		}

        public void SortNotebooks() => SortItems();

		public event EventHandler<NotebooksManager> NotebooksSorted;
		
        public void OnNotebooksSorted()
		{
			NotebooksSorted?.Invoke(this, this);
		}

		
        public async Task<Notebook> NewNotebookAsync(ISearchableStorage<string> storage)
        {
            Notebook notebook = new Notebook() { NotebooksManager = this, Storage = storage };
            if (!await NewItemHelperAsync(notebook, null))
                return null;

			AddItem(notebook);

            SortItems();

			return notebook;
        }

		
        public event EventHandler<Notebook> NotebookLoaded;

		public void OnNotebookLoaded(Notebook notebook, bool anyPageLoaded)
		{
			NotebookLoaded?.Invoke(this, notebook);
		}

		
        public Notebook PreviouslySelectedNotebook { get; private set; }
		public Notebook SelectedNotebook { get; private set; }

        public event EventHandler<Notebook> NotebookSelected;

        public async Task<bool> SelectNotebookAsync(Notebook notebook, bool tryToUnlockPages, bool remember = true)
        {
            if (notebook != null)
            {
                if (!await notebook.LoadAsync(tryToUnlockPages))
                    return false;
            }

            PreviouslySelectedNotebook = SelectedNotebook;
            SelectedNotebook = notebook;
            NotebookSelected?.Invoke(this, SelectedNotebook);

            return true;
        }


		//

		public event EventHandler<Notebook> PagesSorted;

		public void OnPagesSorted(Notebook notebook)
        {
            PagesSorted?.Invoke(this, notebook);
        }

		public event EventHandler<Page> PageLoaded;

		public void OnPageLoaded(Page page, bool anyNoteLoaded)
		{
			PageLoaded?.Invoke(this, page);
		}

        public Page SelectedPage = null;

        public event EventHandler<Page> PageSelected;

        public async Task<bool> SelectPageAsync(Page page, bool tryToUnlockNotes, bool remember = true)
        {
			if (page != null)
			{
				if (!await page.LoadAsync(tryToUnlockNotes))
					return false;
			}

			SelectedPage = page;
            PageSelected?.Invoke(this, SelectedPage);

            return true;
        }


		//

		public event EventHandler<Page> NotesSorted;

		public void OnNotesSorted(Page page)
		{
			NotesSorted?.Invoke(this, page);
		}
		
        public event EventHandler<Note> NoteLoaded;

        public void OnNoteLoaded(Note note)
		{
			NoteLoaded?.Invoke(this, note);
		}

		public async Task<bool> SelectNoteAsync(Note note, bool remember = true)
		{
			return true;
		}


		//

		public void OnItemOpened(Item item)
		{
		}

		public void OnItemSaved(Item item)
		{
		}


        //

		public async Task<bool> NewItemHelperAsync(Item item, Item parent)
		{
			await item.NewAsync(parent);

			item.BatchBegin();

			// Allow user to edit notebook data
			(bool ok, string passwd) rc = await UI.EditItemAsync(item);
			if (!rc.ok)
				return false;

			await item.InitializePasswordAsync(rc.passwd);

			await item.BatchCommitAsync(true);
			return true;
		}

		/// <summary>
		/// Returns true if any item was opened/loaded, false otherwise
		/// </summary>
		public async Task<bool> LoadChildrenForItemHelperAsync<T>(ItemWithItems<T> forWhom, string pattern, bool tryToUnlock) where T : Item, new()
		{
			// TODO: dodac doczytywanie/kasowanie jezeli to co w pamieci nie zgadza sie z tym co na dysku -> synchronizacja
			if (forWhom.Items.Count > 0)
				return false;

			IEnumerable<string> ids = await forWhom.Storage.FindIdsAsync(pattern);
			bool rc = false;
			foreach (var id in ids)
			{
				T item = new T() { NotebooksManager = this };
				await item.OpenAsync(forWhom, id, tryToUnlock);
				forWhom.AddItem(item);
				rc = true;
			}

			return rc;
		}
	}
}
