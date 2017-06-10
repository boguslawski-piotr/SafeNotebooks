using AppKit;

namespace SafeNotebooks.macOS
{
    static class MainClass
    {
		static void Main(string[] args)
		{
#if !DEBUG
			pbXNet.Log.AddLogger(new pbXNet.NSLogLogger());
#endif
			NSApplication.Init();
			NSApplication.SharedApplication.Delegate = new AppDelegate();
			NSApplication.Main(args);
		}
    }
}
