using System;
namespace Tricycle.Media.Models
{
    public struct Range<T> where T : struct
    {
        public T? Min { get; }
        public T? Max { get; }

        public Range(T? min, T? max)
        {
            Min = min;
            Max = max;
        }
    }
}
