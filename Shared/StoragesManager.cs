using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using pbXNet;
using pbXNet.Database;

namespace SafeNotebooks
{
	public class StoragesManager
	{
		public ISerializer Serializer { get; set; }

		public IEnumerable<ISearchableStorage<string>> Storages => _storages?.Values;

		public event EventHandler<ISearchableStorage<string>> StorageAddded;

		protected IDictionary<string, ISearchableStorage<string>> _storages;

		protected readonly string Id;

		public StoragesManager(string id)
		{
			Check.Empty(id, nameof(id));

			Id = id;
		}

		public static async Task<StoragesManager> NewAsync(string id)
		{
			StoragesManager m = new StoragesManager(id);
			await m.InitializeAsync();
			return m;
		}

		public async Task InitializeAsync()
		{
			_storages = new Dictionary<string, ISearchableStorage<string>>();

			try
			{
				foreach (DeviceFileSystem.RootType root in DeviceFileSystem.AvailableRootsForEndUser)
				{
					//await Task.Delay(2000); // simulate network access...

					IFileSystem fs = DeviceFileSystem.New(root);
					IDatabase db = new SDCDatabase(new SqliteConnection($"Data Source={Path.Combine(fs.RootPath, Id)}.db"))
					{
						Name = fs.Name,
						ConnectionType = ConnectionType.Local
					};

					StorageOnFileSystem<string> storage = 
						await StorageOnFileSystem<string>.NewAsync(
							Id,
							await FileSystemInDatabase.NewAsync(Id.Replace(" ", ""), db, true), 
							Serializer
						);

					if (storage != null)
					{
						_storages[db.Name] = storage;
						StorageAddded?.Invoke(this, storage);
					}

				}
			}
			catch (Exception ex)
			{
				await MainWnd.C.DisplayError(ex, this);
			}

			try
			{
#if DEBUG
#if WINDOWS_UWP
				AzureStorageSettings azureStorageSettings = new AzureStorageSettings
				{
					ConnectionString = "UseDevelopmentStorage=true",
					Type = AzureStorageSettings.StorageType.PageBlob,
				};

				StorageOnAzureStorage<string> azureStorage = await StorageOnAzureStorage<string>.NewAsync("Azure Storage Emulator", azureStorageSettings, Serializer);
				if (azureStorage != null)
				{
					_storages[azureStorage.Id] = azureStorage;
					StorageAddded?.Invoke(this, azureStorage);
				}
#endif
#endif
			}
			catch (Exception ex)
			{
				await MainWnd.C.DisplayError(ex, this);
			}
		}

		public async Task<ISearchableStorage<string>> SelectStorageAsync(Func<IEnumerable<ISearchableStorage<string>>, Task<ISearchableStorage<string>>> UI)
		{
			if (_storages.Count <= 0)
				return null;
			else if (_storages.Count == 1)
				return _storages.Values.First();
			return await UI(Storages);
		}
	}
}
