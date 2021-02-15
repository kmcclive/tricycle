using System;
using System.IO.Abstractions;
using System.Linq;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Utilities;

namespace Tricycle.Media.FFmpeg
{
    public class FFmpegConfigManager : FileConfigManager<FFmpegConfig>
    {
        public FFmpegConfigManager(IFileSystem fileSystem,
                                   ISerializer<string> serializer,
                                   string defaultFileName,
                                   string userFileName)
            : base(fileSystem, serializer, defaultFileName, userFileName)
        {

        }

        protected override void Coalesce(FFmpegConfig userConfig, FFmpegConfig defaultConfig)
        {
            if (userConfig.Audio == null)
            {
                userConfig.Audio = defaultConfig.Audio;
            }
            else
            {
                CoalesceAudio(userConfig.Audio, defaultConfig.Audio);
            }

            if (userConfig.Video == null)
            {
                userConfig.Video = defaultConfig.Video;
            }
            else
            {
                CoalesceVideo(userConfig.Video, defaultConfig.Video);
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

                if (string.IsNullOrWhiteSpace(userCodec.Name) && defaultCodec != null)
                {
                    userCodec.Name = defaultCodec.Name;
                }
            }
        }

        void CoalesceVideo(VideoConfig userConfig, VideoConfig defaultConfig)
        {
            if (string.IsNullOrWhiteSpace(userConfig.CropDetectOptions))
            {
                userConfig.CropDetectOptions = defaultConfig.CropDetectOptions;
            }

            if (string.IsNullOrWhiteSpace(userConfig.DeinterlaceOptions))
            {
                userConfig.DeinterlaceOptions = defaultConfig.DeinterlaceOptions;
            }

            if (string.IsNullOrWhiteSpace(userConfig.DenoiseOptions))
            {
                userConfig.DenoiseOptions = defaultConfig.DenoiseOptions;
            }

            if (string.IsNullOrWhiteSpace(userConfig.TonemapOptions))
            {
                userConfig.TonemapOptions = defaultConfig.TonemapOptions;
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

                if (string.IsNullOrWhiteSpace(userCodec.Preset) && defaultCodec != null)
                {
                    userCodec.Preset = defaultCodec.Preset;
                }
            }
        }
    }
}
