using System.Collections.Generic;

namespace Tricycle.Models.Jobs
{
    public class OutputStream
    {
        public int SourceStreamIndex { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public string Tag { get; set; }
        public bool? IsDefault { get; set; }
    }
}
