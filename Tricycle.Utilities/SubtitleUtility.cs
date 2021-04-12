using System;
using Tricycle.Models;

namespace Tricycle.Utilities
{
    public static class SubtitleUtility
    {
        public static bool IsSupportedByContainer(ContainerFormat containerFormat, SubtitleFormat subtitleFormat)
        {
            switch (containerFormat)
            {
                case ContainerFormat.Mkv:
                    switch (subtitleFormat)
                    {
                        case SubtitleFormat.Dvb:
                        case SubtitleFormat.Dvd:
                        case SubtitleFormat.Pgs:
                        case SubtitleFormat.Ssa:
                        case SubtitleFormat.Subrip:
                        case SubtitleFormat.WebVtt:
                            return true;
                        default:
                            return false;
                    }
                case ContainerFormat.Mp4:
                    switch (subtitleFormat)
                    {
                        case SubtitleFormat.TimedText:
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }
    }
}
