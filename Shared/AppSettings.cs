using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pbXNet;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class App : Application
	{
		public static SettingsImpl Settings => new SettingsImpl();

		public class SettingsImpl : Observable
		{
			// Security settings

			const string UnlockUsingDeviceOwnerAuthenticationKey = "_uudoa";
			static readonly bool UnlockUsingDeviceOwnerAuthenticationDefault = false;

			const string UnlockUsingPinKey = "_uup";
			static readonly bool UnlockUsingPinDefault = false;

			const string UsePinAsMasterPasswordKey = "_upamp";
			static readonly bool UsePinAsMasterPasswordDefault = false;

			const string TryToUnlockItemItemsKey = "_ttuii";
			static readonly bool TryToUnlockItemItemsDefault = false;

			public bool UnlockUsingDeviceOwnerAuthentication
			{
				get => GetValueOrDefault<bool>(UnlockUsingDeviceOwnerAuthenticationKey, UnlockUsingDeviceOwnerAuthenticationDefault);
				set => AddOrUpdateValue<bool>(UnlockUsingDeviceOwnerAuthenticationKey, value);
			}

			public bool UnlockUsingPin
			{
				get => GetValueOrDefault<bool>(UnlockUsingPinKey, UnlockUsingPinDefault);
				set => AddOrUpdateValue<bool>(UnlockUsingPinKey, value);
			}

			public bool UsePinAsMasterPassword
			{
				get => GetValueOrDefault<bool>(UsePinAsMasterPasswordKey, UsePinAsMasterPasswordDefault);
				set => AddOrUpdateValue<bool>(UsePinAsMasterPasswordKey, value);
			}

			public bool TryToUnlockItemItems
			{
				get => GetValueOrDefault<bool>(TryToUnlockItemItemsKey, TryToUnlockItemItemsDefault);
				set => AddOrUpdateValue<bool>(TryToUnlockItemItemsKey, value);
			}


			// Direct access to entries

			public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
			{
				return CrossSettings.Current.GetValueOrDefault<T>(key, defaultValue);
			}

			public bool AddOrUpdateValue<T>(string key, T value)
			{
				return CrossSettings.Current.AddOrUpdateValue<T>(key, value);
			}


			// Storage

			public ISearchableStorage<string> Storage = new StorageImpl();

			public class StorageImpl : ISearchableStorage<string>
			{
				public string Id => "2b60a5af8f5e4fa4be62cb05bacbdf2b";

				public string Name => T.Localized("Settings");

				public async Task StoreAsync(string id, string data, DateTime modifiedOn) => CrossSettings.Current.AddOrUpdateValue<string>(id, data);

				public async Task<bool> ExistsAsync(string id) => CrossSettings.Current.Contains(id);

				public async Task<DateTime> GetModifiedOnAsync(string id) => DateTime.MinValue; // TODO: obsluzyc GetModifiedOnAsync

				public async Task DiscardAsync(string id) => CrossSettings.Current.Remove(id);

				public async Task<string> GetACopyAsync(string id)
				{
					string rc = null;
					if (CrossSettings.Current.Contains(id))
						rc = CrossSettings.Current.GetValueOrDefault<string>(id, "");
					return rc;
				}

				public async Task<string> RetrieveAsync(string id)
				{
					string data = await GetACopyAsync(id);
					if (data != null)
						await DiscardAsync(id);
					return data;
				}

				public Task<IEnumerable<string>> FindIdsAsync(string pattern)
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
