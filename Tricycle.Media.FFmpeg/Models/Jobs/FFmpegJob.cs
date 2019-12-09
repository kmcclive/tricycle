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
        [ArgumentPriority(Priority.Input)]
        public string InputFileName { get; set; }

        [ArgumentConverter(typeof(FileNameConverter))]
        [ArgumentPriority(Priority.End)]
        public string OutputFileName { get; set; }

        [Argument("-hide_banner")]
        [ArgumentConverter(typeof(FlagConverter))]
        [ArgumentPriority(Priority.PreInput)]
        public bool HideBanner { get; set; }

        [Argument("-y")]
        [ArgumentConverter(typeof(FlagConverter))]
        [ArgumentPriority(Priority.PreInput)]
        public bool Overwrite { get; set; }

        [Argument("-ss")]
        [ArgumentConverter(typeof(TimeSpanConverter))]
        [ArgumentPriority(Priority.PreInput)]
        public TimeSpan? StartTime { get; set; }

        [Argument("-t")]
        [ArgumentConverter(typeof(TimeSpanConverter))]
        public TimeSpan? Duration { get; set; }

        [Argument("-frames:v")]
        public int? FrameCount { get; set; }

        [Argument("-canvas_size")]
        [ArgumentPriority(Priority.PreInput)]
        public Dimensions? CanvasSize { get; set; }

        [Argument("-forced_subs_only")]
        [ArgumentPriority(Priority.PreInput)]
        [ArgumentConverter(typeof(BinaryConverter))]
        public bool? ForcedSubtitlesOnly { get; set; }

        [Argument("-f")]
        public string Format { get; set; }

        [Argument("-map")]
        [ArgumentConverter(typeof(MappedStreamListConverter))]
        public IList<MappedStream> Streams { get; set; }

        [Argument("-filter_complex")]
        [ArgumentConverter(typeof(FilterListConverter))]
        public IList<IFilter> Filters { get; set; }
    }
}
