using System;
namespace Tricycle.Media.Models
{
    public struct Dimensions
    {
        public int Width { get; }
        public int Height { get; }

        public Dimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override bool Equals(object obj)
        {
            if (obj is Dimensions dimensions)
            {
                return dimensions.Width == Width && dimensions.Height == Height;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + Width;
            hash = hash * 23 + Height;

            return hash;
        }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }
}
