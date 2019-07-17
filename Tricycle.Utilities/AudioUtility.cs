using System.Text.RegularExpressions;
using Tricycle.Models;
using Tricycle.Models.Media;

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

        public static AudioFormat? GetAudioFormat(AudioStreamInfo stream)
        {
            if (string.IsNullOrWhiteSpace(stream?.FormatName))
            {
                return null;
            }

            AudioFormat? result = null;

            if (Regex.IsMatch(stream.FormatName, @"ac(\-)?3", RegexOptions.IgnoreCase))
            {
                result = AudioFormat.Ac3;
            }
            else if (Regex.IsMatch(stream.FormatName, @"aac", RegexOptions.IgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(stream.ProfileName) &&
                    Regex.IsMatch(stream.ProfileName, "HE", RegexOptions.IgnoreCase))
                {
                    result = AudioFormat.HeAac;
                }
                else
                {
                    result = AudioFormat.Aac;
                }
            }

            return result;
        }
    }
}
