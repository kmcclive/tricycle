using System;
using System.Diagnostics;
using AppKit;
using Foundation;
using Tricycle.UI.Models;

namespace Tricycle.UI.macOS
{
    public class MainWindowDelegate : NSWindowDelegate, IAppManager
    {
        public event Action<CancellationArgs> Quitting;

        public override bool WindowShouldClose(NSObject sender)
        {
            var cancellation = new CancellationArgs();

            Quitting?.Invoke(cancellation);

            return !cancellation.Cancel;
        }
    }
}
