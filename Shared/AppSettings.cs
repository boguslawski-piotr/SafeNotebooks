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
		public static class Settings
		{
			public class Storage : ISearchableStorage<string>
			{
				public string Id => "2b60a5af8f5e4fa4be62cb05bacbdf2b";

				public string Name => T.Localized("Settings");

				public async Task StoreAsync(string id, string data, DateTime modifiedOn) => Current.Impl.AddOrUpdateValue<string>(id, data);

				public async Task<bool> ExistsAsync(string id) => Current.Impl.Contains(id);

				public async Task<DateTime> GetModifiedOnAsync(string id) => DateTime.MinValue; // TODO: obsluzyc GetModifiedOnAsync

				public async Task DiscardAsync(string id) => Current.Impl.Remove(id);

				public async Task<string> GetACopyAsync(string id)
				{
					string rc = null;
					if (Current.Impl.Contains(id))
						rc = Current.Impl.GetValueOrDefault<string>(id, "");
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

			public static class Current
			{
				internal static ISettings Impl => CrossSettings.Current;
				public static ISearchableStorage<string> Storage = new Storage();
			}


			// Security settings

			const string UnlockUsingDeviceOwnerAuthenticationKey = "_uudoa";
			static readonly bool UnlockUsingDeviceOwnerAuthenticationDefault = false;

			const string UnlockUsingPinKey = "_uup";
			static readonly bool UnlockUsingPinDefault = true;

			const string TryToUnlockItemItemsKey = "_ttuii";
			static readonly bool TryToUnlockItemItemsDefault = false;

			public static bool UnlockUsingDeviceOwnerAuthentication
			{
				get => Current.Impl.GetValueOrDefault<bool>(UnlockUsingDeviceOwnerAuthenticationKey, UnlockUsingDeviceOwnerAuthenticationDefault);
				set => Current.Impl.AddOrUpdateValue<bool>(UnlockUsingDeviceOwnerAuthenticationKey, value);
			}

			public static bool UnlockUsingPin
			{
				get => Current.Impl.GetValueOrDefault<bool>(UnlockUsingPinKey, UnlockUsingPinDefault);
				set => Current.Impl.AddOrUpdateValue<bool>(UnlockUsingPinKey, value);
			}

			public static bool TryToUnlockItemItems
			{
				get => Current.Impl.GetValueOrDefault<bool>(TryToUnlockItemItemsKey, TryToUnlockItemItemsDefault);
				set => Current.Impl.AddOrUpdateValue<bool>(TryToUnlockItemItemsKey, value);
			}
		}
	}
}
