using System;
namespace Tricycle.Models.Jobs
{
    public class TranscodeStatus
    {
        public TimeSpan Time { get; set; }
        public double FramesPerSecond { get; set; }
        public double Speed { get; set; }
        public long Size { get; set; }
    }
}
