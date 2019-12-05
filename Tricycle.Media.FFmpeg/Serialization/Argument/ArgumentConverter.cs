using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class ArgumentConverter : IArgumentConverter
    {
        public virtual string Convert(string argName, object value)
        {
            return value != null ? $"{argName} {value}" : string.Empty;
        }
    }
}
