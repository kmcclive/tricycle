using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricycle.Diagnostics.Bridge;

namespace Tricycle.UI.UWP
{
    public static class WindowsAppState
    {
        public static IAppServiceConnection Connection { get; set; }
    }
}
