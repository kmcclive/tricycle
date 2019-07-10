using System;
using System.Collections.Generic;

namespace Tricycle.UI.Models
{
    public enum ContainerFormat
    {
        Mp4,
        Mkv
    }

    public class TricycleConfig
    {
        public VideoConfig Video { get; set; }
        public AudioConfig Audio { get; set; }
        public IDictionary<ContainerFormat, string> DefaultFileExtensions { get; set; }
    }
}
