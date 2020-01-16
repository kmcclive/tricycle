using System;
namespace Tricycle.Models.Jobs
{
    public class VideoOutputStream : TranscodedOutputStream<VideoFormat>
    {
        public decimal Quality { get; set; }
        public CropParameters CropParameters { get; set; }
        public Dimensions? ScaledDimensions { get; set; }
        public DynamicRange DynamicRange { get; set; }
        public bool CopyHdrMetadata { get; set; }
        public bool Deinterlace { get; set; }
        public bool Denoise { get; set; }
        public bool Tonemap { get; set; }
    }
}
