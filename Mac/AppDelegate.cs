using AppKit;
using Foundation;

using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

using SafeNotebooks;
using pbXNet;
using System.Reflection;
using CoreGraphics;
using Newtonsoft.Json;

namespace Mac
{
    [Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
	{
		NSWindow window;

		public AppDelegate()
		{
			var settings = NSUserDefaults.StandardUserDefaults;
            const string wt = "__wt";
            const string wl = "__wl";
            const string ww = "__ww";
            const string wh = "__wh";

            CGPoint topleft = new CGPoint()
			{
				Y = settings.IntForKey(wt),
				X = settings.IntForKey(wl)
			};
            topleft.X = topleft.Y = 0;

			CGSize size = new CGSize()
			{
				Height = settings.IntForKey(wh),
				Width = settings.IntForKey(ww)
			};

            size.Height = size.Height <= 0 ? 600 : size.Height;
			size.Width = size.Width <= 0 ? 800 : size.Width;

			CGRect screen = NSScreen.MainScreen.Frame;
            CGRect screenFrame = NSScreen.MainScreen.VisibleFrame;

            //var rect = new CoreGraphics.CGRect(topleft.X <= 0 ? screen.Width / 2 - size.Width / 2 : topleft.X, 
                                               //topleft.Y <= 0 ? screen.Height / 2 - size.Height / 2 : topleft.Y, 
                                               //size.Width, 
                                               //size.Height);
            var rect = new CoreGraphics.CGRect(topleft.X <= 0 ? 100 : topleft.X,
                                               topleft.Y <= 0 ? screen.Height - size.Height - (100 + (screen.Height - screenFrame.Height)) : topleft.Y,
											   size.Width,
											   size.Height);
			var style = NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
			window = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "",
                TitleVisibility = NSWindowTitleVisibility.Hidden,
				MinSize = new CGSize(800, 600),
			};

            //

            window.DidMove += (object sender, System.EventArgs e) =>
            {
                CGRect c = window.ContentRectFor(window.Frame);
				CGRect f = window.Frame;
                settings.SetInt((System.nint)(c.Top + (f.Height - c.Height)), wt);
				settings.SetInt((System.nint)c.Left, wl);
            };

            window.DidResize += (object sender, System.EventArgs e) =>
            {
                CGRect c = window.ContentRectFor(window.Frame);
                CGRect f = window.Frame;
				settings.SetInt((System.nint)c.Width, ww);
                settings.SetInt((System.nint)(c.Height - (f.Height - c.Height)), wh);
			};
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
