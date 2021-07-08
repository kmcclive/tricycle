using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge.Models
{
    public class Response
    {
        public Error Error { get; set; }

        public Response()
        {

        }

        public Response(Error error)
        {
            Error = error;
        }
    }
}
