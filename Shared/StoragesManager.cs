using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pbXNet;

namespace SafeNotebooks
{
	public class StoragesManager
	{
		public ISerializer Serializer { get; set; }

		public IEnumerable<ISearchableStorage<string>> Storages => _storages?.Values;

		protected IDictionary<string, ISearchableStorage<string>> _storages;

		protected string Id;

		public StoragesManager(string id)
		{
			Id = id;
		}

		public static async Task<StoragesManager> NewAsync(string id)
		{
			StoragesManager m = new StoragesManager(id);
			await m.InitializeAsync();
			return m;
		}

		// TODO: obsluga wyjatkow!

		public async Task InitializeAsync()
		{
			_storages = new Dictionary<string, ISearchableStorage<string>>();

			try
			{
				foreach (DeviceFileSystemRoot root in DeviceFileSystem.AvailableRootsForEndUser)
				{
					IFileSystem fs = DeviceFileSystem.New(root);
					StorageOnFileSystem<string> storage = await NewStorageOnFileSystemAsync(fs);
					if (storage != null)
						_storages[fs.Id] = storage;
				}
			}
			catch (Exception ex)
			{
				await MainWnd.C.DisplayError(ex);
			}

			try
			{
#if DEBUG
				IFileSystem fs = await FileSystemInDatabase.NewAsync("Safe Notebooks", new SimpleDatabaseInMemory());
				StorageOnFileSystem<string> storage = await NewStorageOnFileSystemAsync(fs);
				if (storage != null)
					_storages[fs.Id] = storage;
#endif
			}
			catch (Exception ex)
			{
				await MainWnd.C.DisplayError(ex);
			}

			try
			{
#if DEBUG
#if WINDOWS_UWP
				//AzureStorageSettings azureStorageSettings = new AzureStorageSettings
				//{
				//	ConnectionString = "UseDevelopmentStorage=true",
				//	Type = AzureStorageSettings.StorageType.PageBlob,
				//};

				//StorageOnAzureStorage<string> azureStorage = await StorageOnAzureStorage<string>.NewAsync("Azure Storage Emulator", azureStorageSettings, Serializer);
				//if (azureStorage != null)
				//	_storages[azureStorage.Id] = azureStorage;
#endif
#endif
			}
			catch (Exception ex)
			{
				await MainWnd.C.DisplayError(ex);
			}
		}

		async Task<StorageOnFileSystem<string>> NewStorageOnFileSystemAsync(IFileSystem fs)
		{
			try
			{
				StorageOnFileSystem<string> storage = await StorageOnFileSystem<string>.NewAsync(Id, fs, Serializer);
				return storage;
			}
			catch (Exception ex)
			{
				await MainWnd.C.DisplayError(ex);
				return null;
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
