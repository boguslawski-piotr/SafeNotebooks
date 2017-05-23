using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using pbXNet;
using pbXSecurity;

namespace SafeNotebooks
{
    public class NotebooksManager
    {
        public string Id { get; }

        public NotebooksManager(string id)
        {
            Id = id;
        }

        public ISecretsManager SecretsManager { get; set; }

        public INotebooksManagerUI UI { get; set; }

        //

        protected IList<IFileSystem> FileSystems = new List<IFileSystem>();

        public void AddFileSystem(IFileSystem fs)
        {
            FileSystems.Add(fs);
        }

        async Task<IFileSystem> SelectFileSystemAsync()
        {
            if (FileSystems.Count <= 0)
            {
                return new DeviceFileSystem(DeviceFileSystemRoot.Personal);
            }
            else if (FileSystems.Count == 1)
            {
                return FileSystems[0];
            }

            return await UI?.SelectFileSystemAsync(FileSystems);
        }

        protected IDictionary<string, NotebooksStorage> Storages = new Dictionary<string, NotebooksStorage>();

        async Task<NotebooksStorage> GetStorageAsync(IFileSystem fs)
        {
            if (Storages.ContainsKey(fs.Id))
                return Storages[fs.Id];

            NotebooksStorage storage = new NotebooksStorage(Id, fs);
            await storage.InitializeAsync();

            Storages[fs.Id] = storage;
            return storage;
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

            // Save 
            await item.BatchCommitAsync(true);

            return true;
        }

        public async Task LoadChildrenForItemHelperAsync<T>(Item forWhom, ObservableCollection<T> list, string pattern, bool tryToUnlock) where T: Item, new()
        {
			// TODO: dodac doczytywanie/kasowanie jezeli to co w pamieci nie zgadza sie z tym co na dysku -> synchronizacja
			if (list.Count > 0)
				return;
			
            IEnumerable<string> noteIds = await forWhom.Storage.FindIdsAsync(pattern);
            foreach (var id in noteIds)
            {
                T item = new T() { NotebooksManager = this };
                await item.OpenAsync(forWhom, id, tryToUnlock);
		        list.Add(item);
            }
        }

		//

		public ObservableCollection<Notebook> Notebooks { get; private set; } = new ObservableCollection<Notebook>();

        public async Task LoadNotebooksAsync(bool tryToUnlock)
        {
            //

            await SelectNotebookAsync(null, false);
            Notebooks.Clear();

            // Load notebooks from all available file systems

            string pattern = Notebook.IdForStoragePrefix + "\\w*";
            foreach (var fs in FileSystems)
            {
                NotebooksStorage storage = await GetStorageAsync(fs);

				IEnumerable<string> notebookIds = await storage.FindIdsAsync(pattern);
                foreach (var id in notebookIds)
                {
                    Notebook notebook = new Notebook() { NotebooksManager = this, Storage = storage };
                    await notebook.OpenAsync(null, id, tryToUnlock); 

                    Notebooks.Add(notebook);
                }
            }

            // TODO: sortowanie notebooks, pages, notes z zapamietaniem wyboru
            //Notebooks.Sort((a, b) => { return a.DisplayName.CompareTo(b.DisplayName); });
            //Notebooks.Sort((v) => v.DisplayName);
        }

        public async Task<Notebook> NewNotebookAsync()
        {
            IFileSystem fs = await SelectFileSystemAsync();
            if (fs == null)
                return null;

            Notebook notebook = new Notebook() { NotebooksManager = this, Storage = await GetStorageAsync(fs) };
            if (!await NewItemHelperAsync(notebook, null))
                return null;

			Notebooks.Add(notebook);
			// sort notebooks

			return notebook;
        }


        //

        public Notebook SelectedNotebook { get; private set; }

        public event EventHandler<Notebook> NotebookSelected;

        public async Task<bool> SelectNotebookAsync(Notebook notebook, bool tryToUnlockPages, bool remember = true)
        {
            if (notebook != null)
            {
                if (!await notebook.LoadAsync(tryToUnlockPages))
                    return false;
            }

            SelectedNotebook = notebook;
            NotebookSelected?.Invoke(this, SelectedNotebook);

            return true;
        }
		
        public event EventHandler<Notebook> NotebookLoaded;

		public virtual async Task NotebookLoadedAsync(Notebook notebook)
		{
            NotebookLoaded?.Invoke(this, notebook);
		}


		//

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

		public event EventHandler<Page> PageLoaded;

		public virtual async Task PageLoadedAsync(Page page)
        {
            PageLoaded?.Invoke(this, page);
        }
		

		//

		public async Task<bool> SelectNoteAsync(Note note, bool remember = true)
        {
            return true;
        }

        public event EventHandler<Note> NoteLoaded;

		public virtual async Task NoteLoadedAsync(Note note)
		{
			NoteLoaded?.Invoke(this, note);
		}
	}
}
