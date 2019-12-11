using System;
using System.Collections.Generic;
using Tricycle.Media.FFmpeg.Serialization.Argument;
using Tricycle.Models;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class FFmpegJob
    {
        [Argument("-i")]
        [ArgumentConverter(typeof(FileNameConverter))]
        [ArgumentOrder(1)]
        public string InputFileName { get; set; }

        [ArgumentConverter(typeof(FileNameConverter))]
        [ArgumentOrder(3)]
        public string OutputFileName { get; set; }

        [Argument("-hide_banner")]
        [ArgumentConverter(typeof(FlagConverter))]
        [ArgumentOrder(0)]
        public bool HideBanner { get; set; }

        [Argument("-y")]
        [ArgumentConverter(typeof(FlagConverter))]
        [ArgumentOrder(0)]
        public bool Overwrite { get; set; }

        [Argument("-ss")]
        [ArgumentConverter(typeof(TimeSpanConverter))]
        [ArgumentOrder(0)]
        public TimeSpan? StartTime { get; set; }

        [Argument("-t")]
        [ArgumentConverter(typeof(TimeSpanConverter))]
        [ArgumentOrder(2)]
        public TimeSpan? Duration { get; set; }

        [Argument("-frames:vf")]
        [ArgumentOrder(2)]
        public int? FrameCount { get; set; }

        [Argument("-canvas_size")]
        [ArgumentOrder(0)]
        public Dimensions? CanvasSize { get; set; }

        [Argument("-forced_subs_only")]
        [ArgumentConverter(typeof(BinaryConverter))]
        [ArgumentOrder(0)]
        public bool? ForcedSubtitlesOnly { get; set; }

        [Argument("-f")]
        [ArgumentOrder(2)]
        public string Format { get; set; }

        [Argument("-map")]
        [ArgumentConverter(typeof(MappedStreamListConverter))]
        [ArgumentOrder(2)]
        public IList<MappedStream> Streams { get; set; }

        [Argument("-filter_complex")]
        [ArgumentConverter(typeof(FilterListConverter))]
        [ArgumentOrder(2)]
        public IList<IFilter> Filters { get; set; }
    }
}
