namespace Tricycle.Models.Media
{
    public class MasterDisplayProperties
    {
        public Coordinate<int> Red { get; set; }
        public Coordinate<int> Green { get; set; }
        public Coordinate<int> Blue { get; set; }
        public Coordinate<int> WhitePoint { get; set; }
        public Range<int> Luminance { get; set; }
    }
}
