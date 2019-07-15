using System.Collections.Generic;
using Tricycle.Models.Media;

namespace Tricycle.Models.Jobs
{
    public class TranscodeJob
    {
        public MediaInfo SourceInfo { get; set; }
        public ContainerFormat Format { get; set; }
        public string OutputFileName { get; set; }
        public IList<OutputStream> Streams { get; set; }
    }
}
