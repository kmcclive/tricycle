using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge.Models
{
    public class OutputDataMessage : DataMessage<string>
    {
        public OutputDataMessage()
        {

        }

        public OutputDataMessage(int processId)
            : base(processId)
        {

        }

        public OutputDataMessage(int processId, string data)
            : base(processId, data)
        {

        }
    }
}
