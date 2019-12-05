using System;
using System.Collections.Generic;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class Filter
    {
        public string Name { get; set; }
        public IList<IInput> Inputs { get; set; }
        public IList<string> OutputLabels { get; set; }
        public string PrimaryOption { get; set; }
        public IList<FilterOption> Options { get; set; }
        public bool ChainToPrevious { get; set; }
    }
}
