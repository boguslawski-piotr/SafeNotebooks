using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace SafeNotebooks.iOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
#if !DEBUG
			pbXNet.Log.AddLogger(new pbXNet.NSLogLogger());
#endif
			UIApplication.Main(args, null, "AppDelegate");
		}
	}
}
