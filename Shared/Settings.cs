using System;
using System.Collections.Generic;
using System.Text;
using pbXNet;

namespace SafeNotebooks
{
	public class Settings : PlatformSettings
	{
		public Settings(string id, ISerializer serializer = null)
			: base(id, serializer)
		{ }

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
