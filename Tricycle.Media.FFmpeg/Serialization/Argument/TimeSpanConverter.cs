using System;
using System.Text;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class TimeSpanConverter : ArgumentConverter
    {
        public override string Convert(string argName, object value)
        {
            if (value is TimeSpan timeSpan)
            {
                var builder = new StringBuilder();

                if (timeSpan.Hours > 0)
                {
                    builder.Append($"{timeSpan.Hours:00}:");
                }

                builder.Append($"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");

                if (timeSpan.Milliseconds > 0)
                {
                    builder.Append($".{timeSpan.Milliseconds:000}");
                }

                return base.Convert(argName, builder);
            }

            throw new NotSupportedException($"{nameof(value)} must be of type TimeSpan.");
        }
    }
}
