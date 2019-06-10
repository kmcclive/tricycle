namespace Tricycle.Media.Models
{
    public struct Coordinate<T> where T : struct
    {
        public T X { get; }
        public T Y { get; }

        public Coordinate(T x, T y)
        {
            X = x;
            Y = y;
        }
    }
}
