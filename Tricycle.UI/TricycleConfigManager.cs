using System;
using System.IO.Abstractions;
using System.Linq;
using Tricycle.IO;
using Tricycle.Models;
using Tricycle.Models.Config;
using Tricycle.Utilities;

namespace Tricycle.UI
{
    public class TricycleConfigManager : FileConfigManager<TricycleConfig>
    {
        public TricycleConfigManager(IFileSystem fileSystem,
                                     ISerializer<string> serializer,
                                     string defaultFileName,
                                     string userFileName)
            : base(fileSystem, serializer, defaultFileName, userFileName)
        {

        }

        protected override void Coalesce(TricycleConfig userConfig, TricycleConfig defaultConfig)
        {
            var clone = defaultConfig.Clone();

            if (userConfig.Audio == null)
            {
                userConfig.Audio = clone.Audio;
            }
            else
            {
                CoalesceAudio(userConfig.Audio, clone.Audio);
            }

            if (userConfig.Video == null)
            {
                userConfig.Video = clone.Video;
            }
            else
            {
                CoalesceVideo(userConfig.Video, clone.Video);
            }

            if (!Enum.IsDefined(typeof(AutomationMode), userConfig.DestinationDirectoryMode))
            {
                userConfig.DestinationDirectoryMode = defaultConfig.DestinationDirectoryMode;
            }

            if (userConfig.DefaultFileExtensions?.Any() != true)
            {
                userConfig.DefaultFileExtensions = defaultConfig.DefaultFileExtensions;
                return;
            }

            if (defaultConfig.DefaultFileExtensions?.Any() != true)
            {
                return;
            }

            foreach (var pair in userConfig.DefaultFileExtensions.ToList()) // copy the elements so they can be modified
            {
                var format = pair.Key;
                var userExtension = pair.Value;
                var defaultExtension = defaultConfig.DefaultFileExtensions.GetValueOrDefault(format);

                if (string.IsNullOrWhiteSpace(userExtension) && defaultExtension != null)
                {
                    userConfig.DefaultFileExtensions[format] = defaultExtension;
                }
            }
        }

        void CoalesceAudio(AudioConfig userConfig, AudioConfig defaultConfig)
        {
            if (userConfig.Codecs?.Any() != true)
            {
                userConfig.Codecs = defaultConfig.Codecs;
                return;
            }

            if (defaultConfig.Codecs?.Any() != true)
            {
                return;
            }

            foreach (var pair in userConfig.Codecs)
            {
                var format = pair.Key;
                var userCodec = pair.Value;
                var defaultCodec = defaultConfig.Codecs.GetValueOrDefault(format);

                if (string.IsNullOrWhiteSpace(userCodec.Tag) && defaultCodec != null)
                {
                    userCodec.Tag = defaultCodec.Tag;
                }
            }
        }

        void CoalesceVideo(VideoConfig userConfig, VideoConfig defaultConfig)
        {
            if (userConfig.SizeDivisor < 1)
            {
                userConfig.SizeDivisor = defaultConfig.SizeDivisor;
            }

            if (!Enum.IsDefined(typeof(SmartSwitchOption), userConfig.Deinterlace))
            {
                userConfig.Deinterlace = defaultConfig.Deinterlace;
            }

            if (userConfig.Codecs?.Any() != true)
            {
                userConfig.Codecs = defaultConfig.Codecs;
                return;
            }

            if (defaultConfig.Codecs?.Any() != true)
            {
                return;
            }

            foreach (var pair in userConfig.Codecs)
            {
                var format = pair.Key;
                var userCodec = pair.Value;
                var defaultCodec = defaultConfig.Codecs.GetValueOrDefault(format);

                if (defaultCodec != null)
                {
                    userCodec.QualityRange = new Range<decimal>(
                        userCodec.QualityRange.Min ?? defaultCodec.QualityRange.Min,
                        userCodec.QualityRange.Max ?? defaultCodec.QualityRange.Max);

                    if (userCodec.QualitySteps < 1)
                    {
                        userCodec.QualitySteps = defaultCodec.QualitySteps;
                    }

                    if (string.IsNullOrWhiteSpace(userCodec.Tag))
                    {
                        userCodec.Tag = defaultCodec.Tag;
                    }
                }
            }
        }
    }
}
