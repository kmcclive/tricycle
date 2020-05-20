using System.Collections.Generic;

namespace Tricycle.Models.Config
{
    public class TricycleConfig
    {
        public VideoConfig Video { get; set; }
        public AudioConfig Audio { get; set; }
        public bool ForcedSubtitlesOnly { get; set; }
        public IDictionary<ContainerFormat, string> DefaultFileExtensions { get; set; }
        public bool CompletionAlert { get; set; }
        public bool DeleteIncompleteFiles { get; set; }
        public AutomationMode DestinationDirectoryMode { get; set; }
        public string DestinationDirectory { get; set; }
        public bool Debug { get; set; }
    }
}
