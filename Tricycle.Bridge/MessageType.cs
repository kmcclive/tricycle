using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge
{
    public enum MessageType
    {
        StartProcess,
        KillProcess,
        OutputData,
        ErrorData,
        Exited
    }
}
