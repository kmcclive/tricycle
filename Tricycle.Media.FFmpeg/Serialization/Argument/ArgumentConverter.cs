using System;
using System.Globalization;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class ArgumentConverter : IArgumentConverter
    {
        public IArgumentPropertyReflector Reflector { get; set; }

        public virtual string Convert(string argName, object value)
        {
            string stringValue;

            switch (value)
            {
                case decimal decimalValue:
                    stringValue = decimalValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case double doubleValue:
                    stringValue = doubleValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case float floatValue:
                    stringValue = floatValue.ToString(CultureInfo.InvariantCulture);
                    break;
                default:
                    stringValue = value?.ToString();
                    break;
            }

            string delimiter = string.IsNullOrWhiteSpace(argName) || string.IsNullOrWhiteSpace(stringValue)
                               ? string.Empty
                               : " ";

            return $"{argName}{delimiter}{value}";
        }
    }
}
