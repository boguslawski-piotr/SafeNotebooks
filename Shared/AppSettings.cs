using System;
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
            public static ISettings Current
            {
                get { return CrossSettings.Current; }
            }


            public class Storage : IStorage<string>
            {
                public string Id => "2b60a5af8f5e4fa4be62cb05bacbdf2b";

                public string Name => T.Localized("Settings");

				public async Task StoreAsync(string id, string data) => Current.AddOrUpdateValue<string>(id, data);

                public async Task<bool> ExistsAsync(string id) => Current.Contains(id);

				public async Task DiscardAsync(string id) => Current.Remove(id);
				
                public async Task<string> GetACopyAsync(string id)
                {
                    string rc = null;
                    if (Current.Contains(id))
                        rc = Current.GetValueOrDefault<string>(id, "");
                    return rc;
                }

                public async Task<string> RetrieveAsync(string id)
                {
                    string data = await GetACopyAsync(id);
                    await DiscardAsync(id);
                    return data;
                }
            }


            // Security settings

            const string UnlockUsingDeviceOwnerAuthenticationKey = "_uudoa";
            static readonly bool UnlockUsingDeviceOwnerAuthenticationDefault = false;
            const string UnlockUsingPinKey = "_uup";
            static readonly bool UnlockUsingPinDefault = false;
			const string TryToUnlockItemChildrenKey = "_ttuic";
			static readonly bool TryToUnlockItemChildrenDefault = false;

			public static bool UnlockUsingDeviceOwnerAuthentication
            {
                get => Current.GetValueOrDefault<bool>(UnlockUsingDeviceOwnerAuthenticationKey, UnlockUsingDeviceOwnerAuthenticationDefault);
                set => Current.AddOrUpdateValue<bool>(UnlockUsingDeviceOwnerAuthenticationKey, value);
            }

            public static bool UnlockUsingPin
            {
                get => Current.GetValueOrDefault<bool>(UnlockUsingPinKey, UnlockUsingPinDefault);
                set => Current.AddOrUpdateValue<bool>(UnlockUsingPinKey, value);
            }
		
            public static bool TryToUnlockItemChildren
			{
				get => Current.GetValueOrDefault<bool>(TryToUnlockItemChildrenKey, TryToUnlockItemChildrenDefault);
				set => Current.AddOrUpdateValue<bool>(TryToUnlockItemChildrenKey, value);
			}
		}
    }
}
