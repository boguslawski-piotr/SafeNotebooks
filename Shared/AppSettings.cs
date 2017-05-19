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
                public async Task StoreAsync(string id, string data)
                {
                    Current.AddOrUpdateValue<string>(id, data);
                }

                public async Task<bool> ExistsAsync(string id)
                {
                    return Current.Contains(id);
                }

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

                public async Task DiscardAsync(string id)
                {
                    Current.Remove(id);
                }
            }

            // Security

            const string UnlockUsingDeviceOwnerAuthenticationKey = "_uus";
            static readonly bool UnlockUsingDeviceOwnerAuthenticationDefault = false;
            const string UnlockUsingPinKey = "_uup";
            static readonly bool UnlockUsingPinDefault = true;

            public static bool UnlockUsingDeviceOwnerAuthentication
            {
                get { return Current.GetValueOrDefault<bool>(UnlockUsingDeviceOwnerAuthenticationKey, UnlockUsingDeviceOwnerAuthenticationDefault); }
                set { Current.AddOrUpdateValue<bool>(UnlockUsingDeviceOwnerAuthenticationKey, value); }
            }

            public static bool UnlockUsingPin
            {
                get { return Current.GetValueOrDefault<bool>(UnlockUsingPinKey, UnlockUsingPinDefault); }
                set { Current.AddOrUpdateValue<bool>(UnlockUsingPinKey, value); }
            }
        }
    }
}
