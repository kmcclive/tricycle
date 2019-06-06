using System;
using System.Collections.Generic;
using Tricycle.Media.Models;

namespace Tricycle.Media.Models
{
    public class MediaInfo
    {
        public string FormatName { get; set; }
        public TimeSpan Duration { get; set; }
        public IList<StreamInfo> Streams { get; set; }
    }
}
