using System;
using System.Diagnostics;
using AppKit;
using Foundation;
using Tricycle.UI.Models;

namespace Tricycle.UI.macOS
{
    public class MainWindowDelegate : NSWindowDelegate
    {
        IAppManager _appManager;

        public MainWindowDelegate(IAppManager appManager)
        {
            _appManager = appManager;
        }

        public override bool WindowShouldClose(NSObject sender)
        {
            var cancellation = new CancellationArgs();

            _appManager.RaiseQuitting(cancellation);

            return !cancellation.Cancel;
        }
    }
}
