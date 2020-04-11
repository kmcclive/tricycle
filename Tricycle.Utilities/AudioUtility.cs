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
                case AudioMixdown.Mono:
                    return 1;
                case AudioMixdown.Stereo:
                    return 2;
                case AudioMixdown.Surround5dot1:
                    return 6;
                case AudioMixdown.Surround7dot1:
                    return 8;
                default:
                    return 0;
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
                case AudioFormat.DolbyTrueHd:
                    return "Dolby TrueHD";
                case AudioFormat.Dts:
                    return "DTS";
                case AudioFormat.DtsMasterAudio:
                    return "DTS Master Audio";
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
                    return "Surround 5.1";
                case AudioMixdown.Surround7dot1:
                    return "Surround 7.1";
                default:
                    return null;
            }
        }

        public static AudioMixdown? GetMixdown(int channelCount)
        {
            return Enum.GetValues(typeof(AudioMixdown))
                       .Cast<AudioMixdown?>()
                       .FirstOrDefault(m => GetChannelCount(m.Value) == channelCount);
        }

        public static bool IsEncodable(AudioMixdown mixdown)
        {
            switch (mixdown)
            {
                case AudioMixdown.Surround7dot1:
                    return false;
                default:
                    return true;
            }
        }

        public static bool IsSupportedByContainer(ContainerFormat containerFormat, AudioFormat audioFormat)
        {
            switch (containerFormat)
            {
                case ContainerFormat.Mkv:
                    return true;
                case ContainerFormat.Mp4:
                    switch (audioFormat)
                    {
                        case AudioFormat.Aac:
                        case AudioFormat.Ac3:
                        case AudioFormat.Dts:
                        case AudioFormat.HeAac:
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
