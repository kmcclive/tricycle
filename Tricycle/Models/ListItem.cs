using System;
namespace Tricycle.Models
{
    public class ListItem
    {
        public object Value { get; }
        public string Name { get; }

        public ListItem(string name)
            : this(name, name)
        {

        }

        public ListItem(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is ListItem item)
            {
                return object.Equals(Value, item.Value);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 17;
        }

        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }
}
