namespace Tricycle.Models
{
    public class CropParameters
    {
        public Coordinate<int> Start { get; set; }
        public Dimensions Size { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CropParameters cropParameters)
            {
                return Start.Equals(cropParameters.Start) && Size.Equals(cropParameters.Size);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + Start.GetHashCode();
            hash = hash * 23 + Size.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return $"{Start};{Size}";
        }
    }
}
