using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class TimeSpanConverter : ArgumentConverter
    {
        public override string Convert(string argName, object value)
        {
            return base.Convert(argName, $"{value:h':'mm':'ss'.'fff}");
        }
    }
}
