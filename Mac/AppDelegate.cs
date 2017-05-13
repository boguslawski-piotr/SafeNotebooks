using AppKit;
using Foundation;

using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

using SafeNotebooks;

namespace Mac
{

    [Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
	{
		NSWindow window;
		public AppDelegate()
		{
			var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
			var rect = new CoreGraphics.CGRect(800, 500, 1024, 768);
            // TODO: center on screen or restore last (remember: x,y => left-bottom!)

            window = new NSWindow(rect, style, NSBackingStore.Buffered, false);

			window.Title = "Safe Notebooks"; // TODO: translation
			window.TitleVisibility = NSWindowTitleVisibility.Hidden;
		}

		public override NSWindow MainWindow
		{
			get { return window; }
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			Forms.Init();
			LoadApplication(new App());
			base.DidFinishLaunching(notification);
		}
	}
}
