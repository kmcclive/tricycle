using System;
namespace Tricycle.Models.Jobs
{
    public class TranscodeStatus
    {
        public double ProgressPercent { get; set; }
        public TimeSpan ProgressTime { get; set; }
        public double RateFps { get; set; }
        public double RelativeRate { get; set; }
    }
}
