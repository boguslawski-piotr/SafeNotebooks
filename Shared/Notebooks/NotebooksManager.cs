using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

															   if (tasksFinished >= tasks.Count)
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

		Int32 _saveAllTaskRunning = 0;

		async Task SaveAllTask()
		{
			await Task.Delay(1000);

			DateTime sdt = DateTime.Now;

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
				Interlocked.Exchange(ref _saveAllTaskRunning, 0);
			}

			Log.D($"duration: {(DateTime.Now - sdt).Milliseconds}", this);
		}

		public event EventHandler<Item> ItemModifiedOnChanged;

		public void OnItemModifiedOnChanged(Item item)
		{
			if (Interlocked.Exchange(ref _saveAllTaskRunning, 1) == 1)
				return;

			Task.Run(SaveAllTask);

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
			T item = await Item.OpenAsync<T>(this, (parent == this) ? null : parent, storage, idInStorage, tryToUnlock).ConfigureAwait(false);
			if (item != null)
			{
				UI.BeginInvokeOnMainThread(() => parent.AddItem(item));
			}
		}

		async Task ReloadItemAsync(Item item)
		{
			//await Task.Delay(500);
		}

		ConcurrentDictionary<string, CancellationTokenSource> _loadItemsForItemCancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();

		public async Task StartLoadItemsForItemAsync<T>(ItemWithItems forWhom, string pattern, bool tryToUnlock, Action<(int, int)> OnEnd, ISearchableStorage<string> storage = null) where T : Item, new()
		{
			ISearchableStorage<string> storageWithItems = storage ?? forWhom.Storage;

			string cancellationTokenId = forWhom.Id + storageWithItems.Name;

			if (_loadItemsForItemCancellationTokens.TryGetValue(cancellationTokenId, out CancellationTokenSource cts))
			{
				if (cts != null)
				{
					cts.Cancel();

					int retries = 10;
					while (retries-- > 0 && _loadItemsForItemCancellationTokens[cancellationTokenId] != null)
						await Task.Delay(500);
				}
			}

			_loadItemsForItemCancellationTokens[cancellationTokenId] = new CancellationTokenSource();

			await Task.Factory.StartNew(async () =>
			{
				(int itemsAdded, int itemsReloaded) report = (0, 0);
				try
				{
					IEnumerable<string> idsInStorage = await storageWithItems.FindIdsAsync(pattern);
					foreach (var idInStorage in idsInStorage)
					{
						if (_loadItemsForItemCancellationTokens[cancellationTokenId].IsCancellationRequested)
							break;

						T item = (T)forWhom.ObservableItems?.Find((i) => i.IdForStorage == idInStorage);
						if (item == null)
						{
							await OpenAndAddItemAsync<T>(forWhom, storage, idInStorage, tryToUnlock).ConfigureAwait(false);
							report.itemsAdded++;
						}
						else
						{
							if (await storageWithItems.GetModifiedOnAsync(item.IdForStorage).ConfigureAwait(false) > item.ModifiedOn)
							{
								await ReloadItemAsync(item);
								report.itemsReloaded++;
							}
						}

						// TODO: nadal jest zle :( za bardzo to spowalnia wszystko...
						// Trying to make UI more responsive...
						if(storageWithItems.Type < StorageType.RemoteIO)
							await Task.Delay(50).ConfigureAwait(false);
					}

					UI.BeginInvokeOnMainThread(() => OnEnd(report));
				}
				catch (Exception ex)
				{
					await UI.DisplayError(ex, this, nameof(StartLoadItemsForItemAsync)).ConfigureAwait(false);
				}
				finally
				{
					_loadItemsForItemCancellationTokens[cancellationTokenId].Dispose();
					_loadItemsForItemCancellationTokens[cancellationTokenId] = null;
				}
			},
			_loadItemsForItemCancellationTokens[cancellationTokenId].Token
			)
			.ConfigureAwait(false);
		}
	}
}
