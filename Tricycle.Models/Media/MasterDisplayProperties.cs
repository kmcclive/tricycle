namespace Tricycle.Models.Media
{
    public class MasterDisplayProperties
    {
        public Coordinate<decimal> Red { get; set; }
        public Coordinate<decimal> Green { get; set; }
        public Coordinate<decimal> Blue { get; set; }
        public Coordinate<decimal> WhitePoint { get; set; }
        public Range<decimal> Luminance { get; set; }
    }
}
