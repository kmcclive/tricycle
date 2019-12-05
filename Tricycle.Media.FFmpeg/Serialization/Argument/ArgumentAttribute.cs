using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentAttribute : Attribute
    {
        public string Name { get; }

        public ArgumentAttribute(string name)
        {
            Name = name;
        }
    }
}
