using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge.Models
{
    public class DataMessage<T> : Message
    {
        public T Data { get; set; }

        public DataMessage()
        {

        }

        public DataMessage(int processId)
            : base(processId)
        {

        }

        public DataMessage(int processId, T data)
            : this(processId)
        {
            Data = data;
        }
    }
}
