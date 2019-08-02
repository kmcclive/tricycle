using System;
using Tricycle.UI.Models;

namespace Tricycle.UI
{
    public interface IAppManager
    {
        event Action<CancellationArgs> Quitting;
    }
}
