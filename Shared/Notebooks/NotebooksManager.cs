using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using pbXNet;

namespace SafeNotebooks
{
	public class NotebooksManager : ItemWithItems
	{
		public ISerializer Serializer { get; set; }

		public ISecretsManager SecretsManager { get; set; }

		public INotebooksManagerUI UI { get; set; }

		public override string IdForStorage => "M-c4d3404e8bbb4967861357ca00905547";

		public NotebooksManager()
		{
			NotebooksManager = this;
		}

		public static async Task<NotebooksManager> NewAsync(ISearchableStorage<string> storage)
		{
			NotebooksManager m = new NotebooksManager();
			await m.InitializeAsync(storage);
			return m;
		}

		public async Task InitializeAsync(ISearchableStorage<string> storage)
		{
			Storage = storage;
			if (await Storage.ExistsAsync(IdForStorage))
				await OpenAsync(null, IdForStorage, false);
			else
				New(null);
		}


		//

		public event EventHandler NotebooksAreStartingToLoad;
		public event EventHandler<bool> NotebooksLoaded;

		public async Task LoadNotebooksAsync(IEnumerable<ISearchableStorage<string>> storages, bool tryToUnlock)
		{
			NotebooksAreStartingToLoad?.Invoke(this, null);

			string pattern = Notebook.IdForStoragePrefix + "\\w*";
			IList<Task> tasks = new List<Task>();
			bool anyNotebookLoaded = false;

			async Task LoadNotebooksFromStorageAsync(ISearchableStorage<string> storage)
			{
				anyNotebookLoaded |= await LoadItemsForItemAsync<Notebook>(this, pattern, tryToUnlock, storage);
				await Task.Delay(300);
			}

			foreach (var storage in storages)
			{
				tasks.Add(LoadNotebooksFromStorageAsync(storage));
			}

			if (tasks.Count > 0)
				await Task.WhenAll(tasks);

			if (anyNotebookLoaded)
				SortItems();

			NotebooksLoaded?.Invoke(this, anyNotebookLoaded);
		}

		public async Task<Notebook> NewNotebookAsync(ISearchableStorage<string> storage)
		{
			Notebook notebook = await NewItemAsync<Notebook>(null, (n) => n.Storage = storage);
			if (notebook == null)
				return null;

			AddItem(notebook);
			SortItems();

			return notebook;
		}

		public Notebook PreviouslySelectedNotebook { get; private set; }
		public Notebook SelectedNotebook { get; private set; }

		public event EventHandler<Notebook> NotebookWillBeSelected;
		public event EventHandler<Notebook> NotebookSelected;

		public async Task<bool> SelectNotebookAsync(Notebook notebook, bool tryToUnlockPages = false, bool remember = true)
		{
			NotebookWillBeSelected?.Invoke(this, notebook);

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

		public event EventHandler<Notebook> NotebookLoaded;

		public void OnNotebookLoaded(Notebook notebook, bool anyPageLoaded)
		{
			NotebookLoaded?.Invoke(this, notebook);
		}


		//

		public Page PreviouslySelectedPage = null;
		public Page SelectedPage = null;

		public event EventHandler<Page> PageWillBeSelected;
		public event EventHandler<Page> PageSelected;

		public async Task<bool> SelectPageAsync(Page page, bool tryToUnlockNotes = false, bool remember = true)
		{
			PageWillBeSelected?.Invoke(this, page);

			if (page != null)
			{
				if (!await page.LoadAsync(tryToUnlockNotes))
					return false;
			}

			PreviouslySelectedPage = SelectedPage;
			SelectedPage = page;

			PageSelected?.Invoke(this, SelectedPage);
			return true;
		}

		public event EventHandler<Page> PageLoaded;

		public void OnPageLoaded(Page page, bool anyNoteLoaded)
		{
			PageLoaded?.Invoke(this, page);
		}


		//

		public async Task<bool> SelectNoteAsync(Note note, bool remember = true)
		{
			return true;
		}

		public event EventHandler<Note> NoteLoaded;

		public void OnNoteLoaded(Note note)
		{
			NoteLoaded?.Invoke(this, note);
		}


		//

		Object _saveAllTaskRunningLock = new Object();
		volatile bool _saveAllTaskRunning = false;

		async Task SaveAllTask()
		{
			Log.D($"started at {DateTime.Now.ToString("HH:mm:ss.fff")}", this);

			await Task.Delay(1000);
			try
			{
				await NotebooksManager.SaveAllAsync();
			}
			catch (Exception ex)
			{
				Log.E(ex.Message, this);
			}
			finally
			{
				lock (_saveAllTaskRunningLock)
				{
					_saveAllTaskRunning = false;
				}
			}

			Log.D($"ended at {DateTime.Now.ToString("HH:mm:ss.fff")}");
		}

		public event EventHandler<Item> ItemModifiedOnChanged;

		public void OnItemModifiedOnChanged(Item item)
		{
			if (_saveAllTaskRunning)
				return;
			lock (_saveAllTaskRunningLock)
			{
				Task.Run(SaveAllTask);
				_saveAllTaskRunning = true;
			}

			ItemModifiedOnChanged?.Invoke(this, item);
		}

		public event EventHandler<Item> ItemOpened;

		public void OnItemOpened(Item item)
		{
			ItemOpened?.Invoke(this, item);
		}

		public event EventHandler<Item> ItemLoaded;

		public void OnItemLoaded(Item item)
		{
			ItemLoaded?.Invoke(this, item);
		}

		public event EventHandler<Item> ItemSaved;

		public void OnItemSaved(Item item)
		{
			ItemSaved?.Invoke(this, item);
		}

		public event EventHandler<(Item, ItemWithItems)> ItemAdded;

		public void OnItemAdded(Item item, ItemWithItems forWhom)
		{
			ItemAdded?.Invoke(this, (item, forWhom));
		}

		public event EventHandler<ItemWithItems> ItemObservableItemsCreated;

		public void OnItemObservableItemsCreated(ItemWithItems forWhom)
		{
			ItemObservableItemsCreated?.Invoke(this, forWhom);
		}

		public event EventHandler<ItemWithItems> ItemObservableItemsSorted;

		public void OnItemObservableItemsSorted(ItemWithItems forWhom)
		{
			ItemObservableItemsSorted?.Invoke(this, forWhom);
		}


		//

		public async Task<T> NewItemAsync<T>(Item parent, Action<Item> initializer = null) where T : Item, new()
		{
			// TODO: problem: kiedy tworzenie zostanie anulowane (uzytkownik da Anuluj np.) wtedy parent (i jego parent(y))
			// i tak dostana zmodyfikowana date i sie zapisza do storage.
			// Dzieje sie tak dlatego, ze Touch dziala po calej hierarchi (i jest to potrzebne).

			T item = new T()
			{
				NotebooksManager = this
			};

			initializer?.Invoke(item);
			item.New(parent);

			item.BatchBegin();

			// Allow user to edit data
			(bool ok, Password passwd) rc = await UI.EditItemAsync(item);
			if (rc.ok)
			{
				await item.InitializePasswordAsync(rc.passwd);
			}

			item.BatchEnd();
			return rc.ok ? item : null;
		}

		/// <summary>
		/// Returns true if any item was opened/loaded, false otherwise
		/// </summary>
		public async Task<bool> LoadItemsForItemAsync<T>(ItemWithItems forWhom, string pattern, bool tryToUnlock, ISearchableStorage<string> storage = null) where T : Item, new()
		{
			//DateTime s = DateTime.Now;

			async Task CreateItemAsync(string idInStorage)
			{
				T item = new T() { NotebooksManager = this, Storage = storage };
				await item.OpenAsync(forWhom, idInStorage, tryToUnlock);
				//await Task.Delay(300);
				forWhom.AddItem(item);
			}

			async Task ReloadItemAsync(Item item)
			{
				// TODO: co tu zrobic???
			}

			IList<Task> tasks = new List<Task>();
			int tasksExecuted = 0;

			if (storage == null)
				storage = forWhom.Storage;

			IEnumerable<string> idsInStorage = await storage.FindIdsAsync(pattern);
			if (idsInStorage != null)
			{
				int batchSize = 50;
				foreach (var idInStorage in idsInStorage)
				{
					T item = (T)forWhom.Items?.Find((i) => i.IdForStorage == idInStorage);
					if (item == null)
					{
						tasks.Add(CreateItemAsync(idInStorage));
						tasksExecuted++;
					}
					else
					{
						if (await storage.GetModifiedOnAsync(IdForStorage) > item.ModifiedOn)
						{
							tasks.Add(ReloadItemAsync(item));
							tasksExecuted++;
						}
					}

					if (tasks.Count > batchSize)
					{
						await Task.WhenAll(tasks);
						tasks.Clear();

						// Give a little time for UI to refresh content and enter low power mode for the rest
						await Task.Delay(50);
						batchSize = 5;
					}
				}
			}

			if (tasks.Count > 0)
				await Task.WhenAll(tasks);

			//Log.D($"execute time {(DateTime.Now - s).Duration()}");
			return tasksExecuted > 0;
		}
	}
}
