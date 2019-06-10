using System;
using Newtonsoft.Json;

namespace Tricycle.Media.FFmpeg.Models.FFprobe
{
    public class Frame
    {
        [JsonProperty("media_type")]
        public string MediaType { get; set; }

        [JsonProperty("stream_index")]
        public long StreamIndex { get; set; }

        [JsonProperty("key_frame")]
        public long KeyFrame { get; set; }

        [JsonProperty("pkt_pts")]
        public long PktPts { get; set; }

        [JsonProperty("pkt_pts_time")]
        public string PktPtsTime { get; set; }

        [JsonProperty("best_effort_timestamp")]
        public long BestEffortTimestamp { get; set; }

        [JsonProperty("best_effort_timestamp_time")]
        public string BestEffortTimestampTime { get; set; }

        [JsonProperty("pkt_duration")]
        public long PktDuration { get; set; }

        [JsonProperty("pkt_duration_time")]
        public string PktDurationTime { get; set; }

        [JsonProperty("pkt_pos")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long PktPos { get; set; }

        [JsonProperty("pkt_size")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long PktSize { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("pix_fmt")]
        public string PixFmt { get; set; }

        [JsonProperty("sample_aspect_ratio")]
        public string SampleAspectRatio { get; set; }

        [JsonProperty("pict_type")]
        public string PictType { get; set; }

        [JsonProperty("coded_picture_number")]
        public long CodedPictureNumber { get; set; }

        [JsonProperty("display_picture_number")]
        public long DisplayPictureNumber { get; set; }

        [JsonProperty("interlaced_frame")]
        public long InterlacedFrame { get; set; }

        [JsonProperty("top_field_first")]
        public long TopFieldFirst { get; set; }

        [JsonProperty("repeat_pict")]
        public long RepeatPict { get; set; }

        [JsonProperty("color_range")]
        public string ColorRange { get; set; }

        [JsonProperty("color_space")]
        public string ColorSpace { get; set; }

        [JsonProperty("color_primaries")]
        public string ColorPrimaries { get; set; }

        [JsonProperty("color_transfer")]
        public string ColorTransfer { get; set; }

        [JsonProperty("side_data_list")]
        public SideData[] SideDataList { get; set; }
    }
}
