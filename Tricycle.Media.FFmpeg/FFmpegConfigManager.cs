using System;
using System.IO.Abstractions;
using System.Linq;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Models;
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
            var clone = defaultConfig.Clone();

            if (userConfig.Audio == null)
            {
                userConfig.Audio = clone.Audio;
            }
            else
            {
                CoalesceAudio(userConfig.Audio, clone.Audio);
            }

            if (userConfig.Subtitles == null)
            {
                userConfig.Subtitles = clone.Subtitles;
            }
            else
            {
                CoalesceSubtitles(userConfig.Subtitles, clone.Subtitles);
            }

            if (userConfig.Video == null)
            {
                userConfig.Video = clone.Video;
            }
            else
            {
                CoalesceVideo(userConfig.Video, clone.Video, userConfig.Version);
            }

            userConfig.Version = AppState.AppVersion;
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

        void CoalesceSubtitles(SubtitleConfig userConfig, SubtitleConfig defaultConfig)
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

        void CoalesceVideo(VideoConfig userConfig, VideoConfig defaultConfig, Version userVersion)
        {
            if (string.IsNullOrWhiteSpace(userConfig.CropDetectOptions))
            {
                userConfig.CropDetectOptions = defaultConfig.CropDetectOptions;
            }

            if (string.IsNullOrWhiteSpace(userConfig.DeinterlaceOptions))
            {
                userConfig.DeinterlaceOptions = defaultConfig.DeinterlaceOptions;
            }

            if (string.IsNullOrWhiteSpace(userConfig.DenoiseOptions)
                // replace "bad" denoise options from earlier versions
                || (userConfig.DenoiseOptions == "hqdn3d=3:3:4:4")
                    && (userVersion == null || userVersion <= new Version("2.6.0.0")))
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
