using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pbXNet;

namespace SafeNotebooks
{
    public class StoragesManager
    {
        public IList<IFileSystem> FileSystems = new List<IFileSystem>();

        protected IDictionary<string, StorageOnFileSystem<string>> _storages = new Dictionary<string, StorageOnFileSystem<string>>();
        public IEnumerable<StorageOnFileSystem<string>> Storages => _storages?.Values;

        public async Task InitializeAsync()
        {
            foreach (DeviceFileSystemRoot root in DeviceFileSystem.AvailableRootsForEndUser)
            {
                IFileSystem fs = new DeviceFileSystem(root);
                FileSystems.Add(fs);
                _storages[fs.Id] = await NewStorageAsync(fs);
            }
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

		protected async Task<StorageOnFileSystem<string>> NewStorageAsync(IFileSystem fs)
		{
			StorageOnFileSystem<string> storage = new StorageOnFileSystem<string>(App.Name, fs);
			await storage.InitializeAsync();
			return storage;
		}

		public async Task<StorageOnFileSystem<string>> GetStorageAsync(IFileSystem fs)
		{
			if (!_storages.ContainsKey(fs.Id))
				_storages[fs.Id] = await NewStorageAsync(fs);
			return _storages[fs.Id];
		}
		
        public async Task<StorageOnFileSystem<string>> SelectStorageAsync(Func<IEnumerable<StorageOnFileSystem<string>>, Task<StorageOnFileSystem<string>>> UI)
		{
			if (_storages.Count <= 0)
				return null;
			else if (_storages.Count == 1)
				return _storages.Values.First();
			return await UI(Storages);
		}
	}
}
