using System;
using Newtonsoft.Json;

namespace Tricycle.Media.FFmpeg.Models.FFprobe
{
    // Generated using https://app.quicktype.io/
    public class Output
    {
        [JsonProperty("streams")]
        public Stream[] Streams { get; set; }

        [JsonProperty("format")]
        public Format Format { get; set; }
    }
}
