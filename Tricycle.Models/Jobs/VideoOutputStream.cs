using System;
namespace Tricycle.Models.Jobs
{
    public class VideoOutputStream : TranscodedOutputStream<VideoFormat>
    {
        public decimal Quality { get; set; }
        public CropParameters CropParameters { get; set; }
        public Dimensions? ScaledDimensions { get; set; }
        public bool CopyHdrMetadata { get; set; }
        public bool Tonemap { get; set; }
    }
}
