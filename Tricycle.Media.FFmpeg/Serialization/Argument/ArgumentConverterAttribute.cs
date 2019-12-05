using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentConverterAttribute : Attribute
    {
        public Type Converter { get; }

        public ArgumentConverterAttribute(Type converter)
        {
            Converter = converter;
        }
    }
}
