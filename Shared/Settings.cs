using System;
using System.Collections.Generic;
using System.Text;
using pbXNet;

namespace SafeNotebooks
{
	public class Settings : Plugin.pbXSettings.Settings
	{
		// Security settings

		[Default(false)]
		public bool UnlockUsingDOAuthentication { get => Get<bool>(); set => Set(value); }

		[Default(false)]
		public bool UnlockUsingPin { get => Get<bool>(); set => Set(value); }

		[Default(false)]
		public bool UsePinAsMasterPassword { get => Get<bool>(); set => Set(value); }

		[Default(false)]
		public bool TryToUnlockItemItems { get => Get<bool>(); set => Set(value); }
	}
}
