using System;
using System.Collections.Generic;
using System.Linq;

namespace Tricycle.Models.Config
{
    public class TricycleConfig
    {
        public Version Version { get; set; }
        public VideoConfig Video { get; set; }
        public AudioConfig Audio { get; set; }
        public bool ForcedSubtitlesOnly { get; set; }
        public bool PreferSoftSubtitles { get; set; }
        public IDictionary<ContainerFormat, string> DefaultFileExtensions { get; set; }
        public bool CompletionAlert { get; set; }
        public bool DeleteIncompleteFiles { get; set; }
        public AutomationMode DestinationDirectoryMode { get; set; }
        public string DestinationDirectory { get; set; }
        public bool Trace { get; set; }

        public TricycleConfig Clone()
        {
            return new TricycleConfig()
            {
                Version = Version,
                Video = Video?.Clone(),
                Audio = Audio?.Clone(),
                ForcedSubtitlesOnly = ForcedSubtitlesOnly,
                PreferSoftSubtitles = PreferSoftSubtitles,
                DefaultFileExtensions = DefaultFileExtensions?.ToDictionary(p => p.Key, p => p.Value),
                CompletionAlert = CompletionAlert,
                DeleteIncompleteFiles = DeleteIncompleteFiles,
                DestinationDirectoryMode = DestinationDirectoryMode,
                DestinationDirectory = DestinationDirectory,
                Trace = Trace
            };
        }
    }
}
