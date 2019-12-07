using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class BinaryConverter : ArgumentConverter
    {
        public override string Convert(string argName, object value)
        {
            if (value is bool asserted)
            {
                return base.Convert(argName, asserted ? "1" : "0");
            }

            throw new NotSupportedException($"{nameof(value)} must be of type bool.");
        }
    }
}
