using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge.Models
{
    public class DataMessage<T> : Message
    {
        public T Data { get; set; }
    }
}
