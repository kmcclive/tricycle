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
    }
}
