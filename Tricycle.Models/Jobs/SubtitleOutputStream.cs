using System;
namespace Tricycle.Models.Jobs
{
    public class SubtitleOutputStream : TranscodedOutputStream<SubtitleFormat>
    {
        public SubtitleOutputStream()
        {
        }

        public SubtitleOutputStream(SubtitleFormat format)
        {
            Format = format;
        }
    }
}
