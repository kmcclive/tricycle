using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge.Models
{
    public class ErrorDataMessage : DataMessage<string>
    {
        public ErrorDataMessage()
        {

        }

        public ErrorDataMessage(int processId)
            : base(processId)
        {

        }

        public ErrorDataMessage(int processId, string data)
            : base(processId, data)
        {

        }
    }
}
