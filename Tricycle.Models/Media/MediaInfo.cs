using System;
using System.Collections.Generic;

namespace Tricycle.Models.Media
{
    public class MediaInfo
    {
        public string FileName { get; set; }
        public string FormatName { get; set; }
        public TimeSpan Duration { get; set; }
        public IList<StreamInfo> Streams { get; set; }
    }
}
