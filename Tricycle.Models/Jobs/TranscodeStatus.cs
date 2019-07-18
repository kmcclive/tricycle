using System;
namespace Tricycle.Models.Jobs
{
    public class TranscodeStatus
    {
        public double Percent { get; set; }
        public TimeSpan Time { get; set; }
        public double FramesPerSecond { get; set; }
        public double Speed { get; set; }
        public long Size { get; set; }
    }
}
