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

		public IList<IFileSystem> FileSystems { get; private set; }

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
			FileSystems = new List<IFileSystem>();
			_storages = new Dictionary<string, ISearchableStorage<string>>();

			foreach (DeviceFileSystemRoot root in DeviceFileSystem.AvailableRootsForEndUser)
			{
				IFileSystem fs = new DeviceFileSystem(root);
				FileSystems.Add(fs);
				StorageOnFileSystem<string> storage = await NewStorageOnFileSystemAsync(fs);
				if (storage != null)
					_storages[fs.Id] = storage;
			}

#if DEBUG
			AzureStorageSettings azureStorageSettings = new AzureStorageSettings { ConnectionString = "UseDevelopmentStorage=true",	};
			StorageOnAzureStorage<string> azureStorage = await StorageOnAzureStorage<string>.NewAsync("Azure Storage Emulator", azureStorageSettings, Serializer);
			_storages[azureStorage.Id] = azureStorage;
#endif
		}

		//

		public async Task<IFileSystem> SelectFileSystemAsync(Func<IEnumerable<IFileSystem>, Task<IFileSystem>> UI)
		{
			if (FileSystems.Count <= 0)
				return new DeviceFileSystem(DeviceFileSystemRoot.Personal);
			else if (FileSystems.Count == 1)
				return FileSystems[0];
			return await UI(FileSystems);
		}

		//

		protected async Task<StorageOnFileSystem<string>> NewStorageOnFileSystemAsync(IFileSystem fs)
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

		public async Task<ISearchableStorage<string>> GetStorageAsync(IFileSystem fs)
		{
			if (!_storages.ContainsKey(fs.Id))
				_storages[fs.Id] = await NewStorageOnFileSystemAsync(fs);
			return _storages[fs.Id];
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
