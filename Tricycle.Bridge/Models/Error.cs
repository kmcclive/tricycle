using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge.Models
{
    public class  Error
    {
        public string ErrorType { get; set; }
        public string Message { get; set; }

        public Error()
        {

        }

        public Error(string errorType)
        {
            ErrorType = errorType;
        }

        public Error(string errorType, string message)
            : this(errorType)
        {
            Message = message;
        }
    }
}
