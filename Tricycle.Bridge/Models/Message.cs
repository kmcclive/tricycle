using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge.Models
{
    public class Message
    {
        public int ProcessId { get; set; }

        public Message()
        {

        }

        public Message(int processId)
        {
            ProcessId = processId;
        }
    }
}
