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
		}
	}
}
