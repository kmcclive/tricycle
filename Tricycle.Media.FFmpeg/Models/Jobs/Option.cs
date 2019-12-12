using System;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class Option
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Option()
        {

        }

        public Option(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static Option FromName(string name)
        {
            return new Option()
            {
                Name = name
            };
        }

        public static Option FromValue(object value)
        {
            return new Option()
            {
                Value = value?.ToString()
            };
        }
    }
}
