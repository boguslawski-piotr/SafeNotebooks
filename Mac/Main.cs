﻿using AppKit;

namespace Mac
{
    static class MainClass
    {
		static void Main(string[] args)
		{
			NSApplication.Init();
			NSApplication.SharedApplication.Delegate = new AppDelegate(); // add this line
			NSApplication.Main(args);
		}
    }
}
