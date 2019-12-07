using System;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class FilterOption
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public FilterOption()
        {

        }

        public FilterOption(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static FilterOption FromName(string name)
        {
            return new FilterOption()
            {
                Name = name
            };
        }

        public static FilterOption FromValue(object value)
        {
            return new FilterOption()
            {
                Value = value?.ToString()
            };
        }
    }
}
