using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentOrderAttribute : Attribute
    {
        public int Order { get; }

        public ArgumentOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
