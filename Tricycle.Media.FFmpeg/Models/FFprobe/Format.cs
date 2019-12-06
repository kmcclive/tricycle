using System.Collections.Generic;
using Newtonsoft.Json;
using Tricycle.Media.FFmpeg.Serialization.Json;

namespace Tricycle.Media.FFmpeg.Models.FFprobe
{
    // Generated using https://app.quicktype.io/
    public class Format
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("nb_streams")]
        public long NbStreams { get; set; }

        [JsonProperty("nb_programs")]
        public long NbPrograms { get; set; }

        [JsonProperty("format_name")]
        public string FormatName { get; set; }

        [JsonProperty("format_long_name")]
        public string FormatLongName { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("bit_rate")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long BitRate { get; set; }

        [JsonProperty("probe_score")]
        public long ProbeScore { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, string> Tags { get; set; }
    }
}
