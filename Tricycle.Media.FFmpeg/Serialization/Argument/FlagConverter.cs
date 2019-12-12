using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class FlagConverter : IArgumentConverter
    {
        public IArgumentPropertyReflector Reflector { get; set; }

        public string Convert(string argName, object value)
        {
            if (value is bool flag)
            {
                return flag ? argName : string.Empty;
            }

            throw new NotSupportedException($"{nameof(value)} must be of type bool.");
        }
    }
}
