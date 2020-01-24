using System;
using System.Collections.Generic;

namespace Tricycle.Models.Templates
{
    public class JobTemplate
    {
        public VideoTemplate Video { get; set; }
        public SubtitleTemplate Subtitles { get; set; }
        public IList<AudioTemplate> AudioTracks { get; set; }
    }
}
