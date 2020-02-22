using System;
namespace Tricycle.Models.Templates
{
    public class VideoTemplate
    {
        public VideoFormat Format { get; set; }
        public decimal Quality { get; set; }
        public bool Hdr { get; set; }
        public string SizePreset { get; set; }
        public ManualCropTemplate ManualCrop { get; set; }
        public string AspectRatioPreset { get; set; }
        public bool CropBars { get; set; }
        public bool Denoise { get; set; }
    }
}
