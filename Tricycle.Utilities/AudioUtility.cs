using System;
using System.Linq;
using Tricycle.Models;

namespace Tricycle.Utilities
{
    public static class AudioUtility
    {
        public static int GetChannelCount(AudioMixdown mixdown)
        {
            switch (mixdown)
            {
                case AudioMixdown.Stereo:
                    return 2;
                case AudioMixdown.Surround5dot1:
                    return 6;
                case AudioMixdown.Mono:
                default:
                    return 1;
            }
        }

        public static string GetFormatName(AudioFormat format)
        {
            switch (format)
            {
                case AudioFormat.Aac:
                    return "AAC";
                case AudioFormat.Ac3:
                    return "Dolby Digital";
                case AudioFormat.HeAac:
                    return "HE-AAC";
                default:
                    return null;
            }
        }

        public static string GetMixdownName(AudioMixdown mixdown)
        {
            switch (mixdown)
            {
                case AudioMixdown.Mono:
                    return "Mono";
                case AudioMixdown.Stereo:
                    return "Stereo";
                case AudioMixdown.Surround5dot1:
                    return "Surround";
                default:
                    return null;
            }
        }

        public static AudioMixdown? GetMixdown(int channelCount)
        {
            return Enum.GetValues(typeof(AudioMixdown))
                       .Cast<AudioMixdown>()
                       .FirstOrDefault(m => GetChannelCount(m) == channelCount);
        }
    }
}
