using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class ArgumentProperty
    {
        public string PropertyName { get; set; }
        public string ArgumentName { get; set; }
        public IArgumentConverter Converter { get; set; }
        public bool HasCustomConverter { get; set; }
        public object Value { get; set; }
    }
}
