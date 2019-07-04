using System;
using Tricycle.Media.Models;

namespace Tricycle.Models
{
    public enum VideoFormat
    {
        Avc,
        Hevc
    }

    public class VideoCodec
    {
        public VideoFormat Format { get; set; }
        public Range<decimal> QualityRange { get; set; }
        public int QualitySteps { get; set; }
    }
}
