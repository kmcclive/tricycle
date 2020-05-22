using System;
using System.Diagnostics;

namespace Tricycle.Diagnostics
{
    public class TimestampTextWriterTraceListener : TextWriterTraceListener
    {
        public TimestampTextWriterTraceListener(string fileName)
            : base(fileName)
        {

        }

        public override void WriteLine(string message)
        {
            base.WriteLine($"{DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)} {message}");
        }
    }
}
