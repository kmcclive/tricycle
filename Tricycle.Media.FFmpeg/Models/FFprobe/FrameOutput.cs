using System;
using Newtonsoft.Json;

namespace Tricycle.Media.FFmpeg.Models.FFprobe
{
    public class FrameOutput
    {
        [JsonProperty("frames")]
        public Frame[] Frames { get; set; }
    }
}
