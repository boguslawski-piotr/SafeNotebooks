using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pbXNet;
using Xamarin.Forms;

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
				await OpenAsync(false);
			else
				InternalNew();
		}


		//

		public event EventHandler NotebooksAreStartingToLoad;
		public event EventHandler<(int notebooksAdded, int notebooksReloaded)> NotebooksLoaded;

		public async Task LoadNotebooksAsync(IEnumerable<ISearchableStorage<string>> storages, bool tryToUnlock)
		{
			NotebooksAreStartingToLoad?.Invoke(this, null);

			(int notebooksAdded, int notebooksReloaded) report = (0, 0);
			string pattern = Notebook.IdForStoragePrefix + "\\w*";
			IList<Task> tasks = new List<Task>();
			int tasksFinished = 0;

			async Task LoadNotebooksFromStorageAsync(ISearchableStorage<string> storage)
			{
				await StartLoadItemsForItemAsync<Notebook>(this, pattern, tryToUnlock,
														   ((int notebooksAdded, int notebooksReloaded) r) =>
														   {
															   report.notebooksAdded += r.notebooksAdded;
															   report.notebooksReloaded += r.notebooksReloaded;

															   tasksFinished++;

															   if(tasksFinished >= tasks.Count)
																   NotebooksLoaded?.Invoke(this, report);
														   },
														   storage);
			}

			foreach (var storage in storages)
			{
				tasks.Add(LoadNotebooksFromStorageAsync(storage));
			}

			if (tasks.Count > 0)
				await Task.WhenAll(tasks);
		}

		public async Task<Notebook> NewNotebookAsync(ISearchableStorage<string> storage)
		{
			Notebook notebook = await NewItemAsync<Notebook>(null, (n) => n.Storage = storage);
			if (notebook == null)
				return null;

			AddItem(notebook);

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

		public void OnNotebookLoaded(Notebook notebook, (int pagesAdded, int pagesReloaded) report)
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

		public void OnPageLoaded(Page page, (int notesAdded, int notesReloaded) report)
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

			T item = Item.New<T>(this, parent);
			initializer?.Invoke(item);

			item.BatchBegin();

			// Allow user to edit data
			(bool ok, IPassword passwd) rc = await UI.EditItemAsync(item);
			if (rc.ok)
			{
				await item.InitializePasswordAsync(rc.passwd);
			}

			item.BatchEnd();
			return rc.ok ? item : null;
		}

		async Task OpenAndAddItemAsync<T>(ItemWithItems parent, ISearchableStorage<string> storage, string idInStorage, bool tryToUnlock) where T : Item, new()
		{
			T item = await Item.OpenAsync<T>(this, parent, storage, idInStorage, tryToUnlock);
			if (item != null)
			{
				Device.BeginInvokeOnMainThread(() => parent.AddItem(item));
				//parent.AddItem(item);
			}
		}

		async Task ReloadItemAsync(Item item)
		{
			// TODO: co tu zrobic???
		}

		/// <summary>
		/// Returns true if any item was opened/loaded, false otherwise
		/// </summary>
		public async Task<(int, int)> LoadItemsForItemAsync<T>(ItemWithItems forWhom, string pattern, bool tryToUnlock, ISearchableStorage<string> storage = null) where T : Item, new()
		{
			ISearchableStorage<string> storageWithItems = storage ?? forWhom.Storage;
			IList<IList<Task>> allTasks = new List<IList<Task>>();

			(int itemsAdded, int itemsReloaded) report = (0, 0);
			int tasksScheduled = 0;

			IEnumerable<string> idsInStorage = await storageWithItems.FindIdsAsync(pattern);
			if (idsInStorage != null)
			{
				const int batchSize = 128;
				IList<Task> tasks = new List<Task>();

				foreach (var idInStorage in idsInStorage)
				{
					T item = (T)forWhom.ObservableItems?.Find((i) => i.IdForStorage == idInStorage);
					if (item == null)
					{
						tasks.Add(OpenAndAddItemAsync<T>(forWhom, storage, idInStorage, tryToUnlock));
						tasksScheduled++;
						report.itemsAdded++;
					}
					else
					{
						if (await storageWithItems.GetModifiedOnAsync(item.IdForStorage) > item.ModifiedOn)
						{
							tasks.Add(ReloadItemAsync(item));
							tasksScheduled++;
							report.itemsReloaded++;
						}
					}

					if (tasks.Count > batchSize)
					{
						allTasks.Add(tasks);
						tasks = new List<Task>();
					}
				}
			}

			if (tasksScheduled > 0)
			{
				foreach (var tasks in allTasks)
				{
					await Task.WhenAll(tasks);
				}
			}

			return report;
		}

		public async Task StartLoadItemsForItemAsync<T>(ItemWithItems forWhom, string pattern, bool tryToUnlock, Action<(int, int)> OnEnd, ISearchableStorage<string> storage = null) where T : Item, new()
		{
			ISearchableStorage<string> storageWithItems = storage ?? forWhom.Storage;

			// On Windows (UWP) we have true async IO even for local file system, not faked one like on other systems.
			// That's why the first data load algorithm is off.

#if !WINDOWS_UWP

			if ((StorageType.Quick & storageWithItems.Type) == storageWithItems.Type)
			{
				OnEnd(await LoadItemsForItemAsync<T>(forWhom, pattern, tryToUnlock, storage));
				return;
			}

#endif

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			Task.Run(async () =>
			{
				(int itemsAdded, int itemsReloaded) report = (0, 0);

				IEnumerable<string> idsInStorage = await storageWithItems.FindIdsAsync(pattern);
				if (idsInStorage != null)
				{
					foreach (var idInStorage in idsInStorage)
					{
						const int batchSize = 32;

						T item = (T)forWhom.ObservableItems?.Find((i) => i.IdForStorage == idInStorage);
						if (item == null)
						{
							await OpenAndAddItemAsync<T>(forWhom, storage, idInStorage, tryToUnlock);
							report.itemsAdded++;
						}
						else
						{
							if (await storageWithItems.GetModifiedOnAsync(item.IdForStorage) > item.ModifiedOn)
							{
								await ReloadItemAsync(item);
								report.itemsReloaded++;
							}
						}

						// Trying to make UI more responsive...
						if (report.itemsAdded > batchSize)
							await Task.Delay(25);
					}
				}

				Device.BeginInvokeOnMainThread(() => OnEnd(report));
			});

#pragma warning restore CS4014
		}
	}
}
