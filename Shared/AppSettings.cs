using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using pbXNet;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class App : Application
	{
		public static SettingsImpl Settings { get; } = new SettingsImpl();

		public class SettingsImpl : Observable
		{
			// Security settings

			const string UnlockUsingDOAuthenticationKey = "_uudoa";
			static readonly bool UnlockUsingDOAuthenticationDefault = false;

			const string UnlockUsingPinKey = "_uup";
			static readonly bool UnlockUsingPinDefault = false;

			const string UsePinAsMasterPasswordKey = "_upamp";
			static readonly bool UsePinAsMasterPasswordDefault = false;

			const string TryToUnlockItemItemsKey = "_ttuii";
			static readonly bool TryToUnlockItemItemsDefault = false;

			public bool UnlockUsingDOAuthentication
			{
				get => GetValueOrDefault(UnlockUsingDOAuthenticationKey, UnlockUsingDOAuthenticationDefault);
				set => AddOrUpdateValue(UnlockUsingDOAuthenticationKey, value);
			}

			public bool UnlockUsingPin
			{
				get => GetValueOrDefault(UnlockUsingPinKey, UnlockUsingPinDefault);
				set => AddOrUpdateValue(UnlockUsingPinKey, value);
			}

			public bool UsePinAsMasterPassword
			{
				get => GetValueOrDefault(UsePinAsMasterPasswordKey, UsePinAsMasterPasswordDefault);
				set => AddOrUpdateValue(UsePinAsMasterPasswordKey, value);
			}

			public bool TryToUnlockItemItems
			{
				get => GetValueOrDefault(TryToUnlockItemItemsKey, TryToUnlockItemItemsDefault);
				set => AddOrUpdateValue(TryToUnlockItemItemsKey, value);
			}


			// Direct access to entries

			bool GetValueOrDefault(string key, bool defaultValue = false)
			{
				return CrossSettings.Current.GetValueOrDefault(key, defaultValue);
			}

			IDictionary<string, object> _values = new Dictionary<string, object>();

			void _AddOrUpdateValue(string key, object value, [CallerMemberName]string name = null)
			{
				// This piece is only for the fully functional Observable interface.
				_values.TryGetValue(key, out object storage);
				SetValue(ref storage, value, name);
				_values[key] = storage;
			}

			bool AddOrUpdateValue(string key, bool value, [CallerMemberName]string name = null)
			{
				_AddOrUpdateValue(key, value, name);
				return CrossSettings.Current.AddOrUpdateValue(key, value);
			}


			// Storage

			public IStorage<string> Storage = new StorageImpl();

			public class StorageImpl : IStorage<string>
			{
				public StorageType Type => StorageType.Memory;

				public string Id => "2b60a5af8f5e4fa4be62cb05bacbdf2b";

				public string Name => T.Localized("Settings");

				public Task InitializeAsync() => Task.FromResult(true);

				public Task StoreAsync(string id, string data, DateTime modifiedOn)
				{
					CrossSettings.Current.AddOrUpdateValue(id, data); 
					return Task.FromResult(true);
				}

				public Task<bool> ExistsAsync(string id) => Task.FromResult(CrossSettings.Current.Contains(id));

				public Task<DateTime> GetModifiedOnAsync(string id) => Task.FromResult(DateTime.MinValue); // TODO: obsluzyc GetModifiedOnAsync

				public Task DiscardAsync(string id)
				{
					CrossSettings.Current.Remove(id);
					return Task.FromResult(true);
				}

				public Task<string> GetACopyAsync(string id)
				{
					string rc = null;
					if (CrossSettings.Current.Contains(id))
						rc = CrossSettings.Current.GetValueOrDefault(id, "");
					return Task.FromResult(rc);
				}

				public async Task<string> RetrieveAsync(string id)
				{
					string data = await GetACopyAsync(id);
					if (data != null)
						await DiscardAsync(id);
					return data;
				}
			}
		}
	}
}
