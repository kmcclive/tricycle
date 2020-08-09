using System.Collections.Generic;
using Newtonsoft.Json;
using Tricycle.Media.FFmpeg.Serialization.Json;

namespace Tricycle.Media.FFmpeg.Models.FFprobe
{
    // Generated using https://app.quicktype.io/
    public class Stream
    {
        [JsonProperty("index")]
        public long Index { get; set; }

        [JsonProperty("codec_name")]
        public string CodecName { get; set; }

        [JsonProperty("codec_long_name")]
        public string CodecLongName { get; set; }

        [JsonProperty("profile", NullValueHandling = NullValueHandling.Ignore)]
        public string Profile { get; set; }

        [JsonProperty("codec_type")]
        public string CodecType { get; set; }

        [JsonProperty("codec_time_base")]
        public string CodecTimeBase { get; set; }

        [JsonProperty("codec_tag_string")]
        public string CodecTagString { get; set; }

        [JsonProperty("codec_tag")]
        public string CodecTag { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("coded_width", NullValueHandling = NullValueHandling.Ignore)]
        public long? CodedWidth { get; set; }

        [JsonProperty("coded_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? CodedHeight { get; set; }

        [JsonProperty("has_b_frames", NullValueHandling = NullValueHandling.Ignore)]
        public long? HasBFrames { get; set; }

        [JsonProperty("sample_aspect_ratio", NullValueHandling = NullValueHandling.Ignore)]
        public string SampleAspectRatio { get; set; }

        [JsonProperty("display_aspect_ratio", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayAspectRatio { get; set; }

        [JsonProperty("pix_fmt", NullValueHandling = NullValueHandling.Ignore)]
        public string PixFmt { get; set; }

        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public long? Level { get; set; }

        [JsonProperty("color_range", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorRange { get; set; }

        [JsonProperty("color_space", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorSpace { get; set; }

        [JsonProperty("color_transfer", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorTransfer { get; set; }

        [JsonProperty("color_primaries", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorPrimaries { get; set; }

        [JsonProperty("refs", NullValueHandling = NullValueHandling.Ignore)]
        public long? Refs { get; set; }

        [JsonProperty("r_frame_rate")]
        public string RFrameRate { get; set; }

        [JsonProperty("avg_frame_rate")]
        public string AvgFrameRate { get; set; }

        [JsonProperty("time_base")]
        public string TimeBase { get; set; }

        [JsonProperty("start_pts")]
        public long StartPts { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("disposition")]
        public Dictionary<string, long> Disposition { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, string> Tags { get; set; }

        [JsonProperty("sample_fmt", NullValueHandling = NullValueHandling.Ignore)]
        public string SampleFmt { get; set; }

        [JsonProperty("sample_rate", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? SampleRate { get; set; }

        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public long? Channels { get; set; }

        [JsonProperty("channel_layout", NullValueHandling = NullValueHandling.Ignore)]
        public string ChannelLayout { get; set; }

        [JsonProperty("bits_per_sample", NullValueHandling = NullValueHandling.Ignore)]
        public long? BitsPerSample { get; set; }

        [JsonProperty("bits_per_raw_sample", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? BitsPerRawSample { get; set; }

        [JsonProperty("bit_rate", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? BitRate { get; set; }

        [JsonProperty("duration_ts", NullValueHandling = NullValueHandling.Ignore)]
        public long? DurationTs { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public string Duration { get; set; }

        [JsonProperty("side_data_list")]
        public SideData[] SideDataList { get; set; }
    }
}
