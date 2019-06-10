using System;
using Newtonsoft.Json;

namespace Tricycle.Media.FFmpeg.Models.FFprobe
{
    public class SideData
    {
        [JsonProperty("side_data_type")]
        public string SideDataType { get; set; }

        [JsonProperty("red_x", NullValueHandling = NullValueHandling.Ignore)]
        public string RedX { get; set; }

        [JsonProperty("red_y", NullValueHandling = NullValueHandling.Ignore)]
        public string RedY { get; set; }

        [JsonProperty("green_x", NullValueHandling = NullValueHandling.Ignore)]
        public string GreenX { get; set; }

        [JsonProperty("green_y", NullValueHandling = NullValueHandling.Ignore)]
        public string GreenY { get; set; }

        [JsonProperty("blue_x", NullValueHandling = NullValueHandling.Ignore)]
        public string BlueX { get; set; }

        [JsonProperty("blue_y", NullValueHandling = NullValueHandling.Ignore)]
        public string BlueY { get; set; }

        [JsonProperty("white_point_x", NullValueHandling = NullValueHandling.Ignore)]
        public string WhitePointX { get; set; }

        [JsonProperty("white_point_y", NullValueHandling = NullValueHandling.Ignore)]
        public string WhitePointY { get; set; }

        [JsonProperty("min_luminance", NullValueHandling = NullValueHandling.Ignore)]
        public string MinLuminance { get; set; }

        [JsonProperty("max_luminance", NullValueHandling = NullValueHandling.Ignore)]
        public string MaxLuminance { get; set; }

        [JsonProperty("max_content", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxContent { get; set; }

        [JsonProperty("max_average", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxAverage { get; set; }
    }
}
