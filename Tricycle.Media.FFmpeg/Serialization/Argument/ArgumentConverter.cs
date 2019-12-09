using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class ArgumentConverter : IArgumentConverter
    {
        public IArgumentPropertyReflector Reflector { get; set; }

        public virtual string Convert(string argName, object value)
        {
            string stringValue = value?.ToString();
            string delimiter = string.IsNullOrWhiteSpace(argName) || string.IsNullOrWhiteSpace(stringValue)
                               ? string.Empty
                               : " ";

            return $"{argName}{delimiter}{value}";
        }
    }
}
