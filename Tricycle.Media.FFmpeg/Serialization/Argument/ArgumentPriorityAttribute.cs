using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public enum Priority
    {
        PreInput,
        PostInput
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentPriorityAttribute : Attribute
    {
        public Priority Priority { get; }

        public ArgumentPriorityAttribute(Priority priority)
        {
            Priority = priority;
        }
    }
}
