using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge.Models
{
    public class ExitedMessage : Message
    {
        public int ExitCode { get; set; }

        public ExitedMessage()
        {

        }

        public ExitedMessage(int processId)
            : base(processId)
        {

        }

        public ExitedMessage(int processId, int exitCode)
            : this(processId)
        {
            ExitCode = exitCode;
        }
    }
}
