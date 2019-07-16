﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tricycle.Diagnostics.Utilities;
using Tricycle.Media.FFmpeg.Models;
using Tricycle.Models;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;
using Tricycle.Utilities;

namespace Tricycle.Media.FFmpeg
{
    public class FFmpegArgumentGenerator : IFFmpegArgumentGenerator
    {
        #region Fields

        readonly IProcessUtility _processUtility;
        readonly FFmpegConfig _config;

        #endregion

        #region Constructors

        public FFmpegArgumentGenerator(IProcessUtility processUtility, FFmpegConfig config)
        {
            _processUtility = processUtility;
            _config = config;
        }

        #endregion

        #region Methods

        #region Public

        public string GenerateArguments(TranscodeJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (job.SourceInfo == null)
            {
                throw new ArgumentException($"{nameof(job)}.{nameof(job.SourceInfo)} is null.", nameof(job));
            }

            if (string.IsNullOrWhiteSpace(job.SourceInfo.FileName))
            {
                throw new ArgumentException(
                    $"{nameof(job)}.{nameof(job.SourceInfo)}.{nameof(job.SourceInfo.FileName)} is null or empty.",
                    nameof(job));
            }

            if (job.SourceInfo.Streams?.Any() != true)
            {
                throw new ArgumentException(
                    $"{nameof(job)}.{nameof(job.SourceInfo)}.{nameof(job.SourceInfo.Streams)} is null or empty.",
                    nameof(job));
            }

            if (string.IsNullOrWhiteSpace(job.OutputFileName))
            {
                throw new ArgumentException($"{nameof(job)}.{nameof(job.OutputFileName)} is null or empty.", nameof(job));
            }

            if (job.Streams?.Any() != true)
            {
                throw new ArgumentException($"{nameof(job)}.{nameof(job.Streams)} is null or empty.", nameof(job));
            }

            var argBuilder = new StringBuilder();

            AppendUniversalArguments(argBuilder);
            AppendDelimiter(argBuilder);
            AppendInput(argBuilder, job.SourceInfo.FileName);
            AppendDelimiter(argBuilder);
            AppendFormat(argBuilder, job.Format);
            AppendDelimiter(argBuilder);

            try
            {
                AppendStreamMap(argBuilder, job.SourceInfo, job.Streams);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(
                    $"{nameof(job)}.{nameof(job.SourceInfo)}.{nameof(job.SourceInfo.Streams)} is missing data.",
                    nameof(job),
                    ex);
            }

            AppendDelimiter(argBuilder);
            AppendOutput(argBuilder, job.OutputFileName);

            return argBuilder.ToString();
        }

        #endregion

        #region Private

        #region Universal

        void AppendUniversalArguments(StringBuilder builder)
        {
            builder.Append("-hide_banner -y");
        }

        void AppendDelimiter(StringBuilder builder)
        {
            builder.Append(" ");
        }

        void AppendOption(StringBuilder builder, string key, string value)
        {
            builder.Append($"{key}={value}");
        }

        void AppendOptionDelimiter(StringBuilder builder)
        {
            builder.Append(":");
        }

        void AppendListDelimiter(StringBuilder builder)
        {
            builder.Append(",");
        }

        void AppendInput(StringBuilder builder, string fileName)
        {
            builder.Append($"-i {_processUtility.EscapeFilePath(fileName)}");
        }

        void AppendOutput(StringBuilder builder, string fileName)
        {
            builder.Append($"{_processUtility.EscapeFilePath(fileName)}");
        }

        void AppendFormat(StringBuilder builder, ContainerFormat format)
        {
            string container;

            switch (format)
            {
                case ContainerFormat.Mkv:
                    container = "matroska";
                    break;
                case ContainerFormat.Mp4:
                    container = "mp4";
                    break;
                default:
                    throw new NotSupportedException($"The container format {format} is not supported.");
            }

            builder.Append($"-f ${container}");
        }

        void AppendStreamMap(StringBuilder builder, MediaInfo sourceInfo, IList<OutputStream> streams)
        {
            IDictionary<int, StreamInfo> sourceStreamsByIndex = sourceInfo.Streams.ToDictionary(s => s.Index);
            int audioIndex = 0;
            int videoIndex = 0;

            for (int i = 0; i < streams.Count; i++)
            {
                OutputStream outputStream = streams[i];
                StreamInfo sourceStream;

                if (!sourceStreamsByIndex.TryGetValue(outputStream.SourceStreamIndex, out sourceStream))
                {
                    throw new ArgumentException(
                        $"{nameof(sourceInfo)} does not contain a stream with index {outputStream.SourceStreamIndex}.",
                        nameof(sourceInfo));
                }

                if (i > 0)
                {
                    AppendDelimiter(builder);
                }

                int relativeIndex;

                switch(sourceStream.StreamType)
                {
                    case StreamType.Audio:
                        relativeIndex = audioIndex;
                        audioIndex++;
                        break;
                    case StreamType.Video:
                        relativeIndex = videoIndex;
                        videoIndex++;
                        break;
                    default:
                        throw new NotSupportedException($"The stream type {sourceStream.StreamType} is not supported.");
                }

                AppendStream(builder, sourceStream, outputStream, relativeIndex);
            }
        }

        void AppendStream(StringBuilder builder,
                          StreamInfo sourceStream,
                          OutputStream outputStream,
                          int relativeIndex)
        {
            builder.Append($"-map 0:{sourceStream.Index}");
            AppendDelimiter(builder);

            string streamSpecifier = GetStreamSpecifier(sourceStream.StreamType, relativeIndex);

            switch (outputStream)
            {
                case VideoOutputStream video:
                    if (sourceStream is VideoStreamInfo videoSource)
                    {
                        AppendVideoStream(builder, videoSource, video, streamSpecifier);
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"{nameof(sourceStream)} and {nameof(outputStream)} types do not match.",
                            nameof(outputStream));
                    }
                    break;
                case AudioOutputStream audio:
                    if (sourceStream is AudioStreamInfo == false)
                    {
                        throw new ArgumentException(
                            $"{nameof(sourceStream)} and {nameof(outputStream)} types do not match.",
                            nameof(outputStream));
                    }
                    AppendAudioStream(builder, audio, streamSpecifier);
                    break;
                default:
                    AppendPassthruStream(builder, streamSpecifier);
                    break;
            }
        }

        void AppendPassthruStream(StringBuilder builder, string streamSpecifier)
        {
            builder.Append($"-c:{streamSpecifier} copy");
        }

        string GetStreamSpecifier(StreamType streamType, int relativeIndex)
        {
            return $"{(streamType == StreamType.Video ? "v" : "a")}:{relativeIndex}";
        }

        #endregion

        #region Audio

        void AppendAudioStream(StringBuilder builder, AudioOutputStream outputStream, string streamSpecifier)
        {
            AppendAudioFormat(builder, streamSpecifier, outputStream.Format);

            if (outputStream.Mixdown.HasValue)
            {
                AppendDelimiter(builder);
                AppendAudioMixdown(builder, streamSpecifier, outputStream.Mixdown.Value);
            }

            if (outputStream.Quality.HasValue)
            {
                AppendDelimiter(builder);
                AppendAudioQuality(builder, streamSpecifier, outputStream.Quality.Value);
            }
        }

        void AppendAudioFormat(StringBuilder builder, string streamSpecifier, AudioFormat format)
        {
            AudioCodec codec = GetAudioCodec(format);

            builder.Append($"-c:{streamSpecifier} {codec.Name}");

            if (!string.IsNullOrWhiteSpace(codec.Options))
            {
                AppendDelimiter(builder);
                builder.Append(codec.Options);
            }
        }

        void AppendAudioMixdown(StringBuilder builder, string streamSpecifier, AudioMixdown mixdown)
        {
            builder.Append($"-ac:{streamSpecifier} {AudioUtility.GetChannelCount(mixdown)}");
        }

        void AppendAudioQuality(StringBuilder builder, string streamSpecifier, decimal quality)
        {
            builder.Append($"-b:{streamSpecifier} {quality}k");
        }

        AudioCodec GetAudioCodec(AudioFormat format)
        {
            if ((_config.Audio?.Codecs != null) && _config.Audio.Codecs.TryGetValue(format, out var codec))
            {
                return codec;
            }

            switch (format)
            {
                case AudioFormat.Aac:
                    return new AudioCodec("aac");
                case AudioFormat.Ac3:
                    return new AudioCodec("ac3");
                case AudioFormat.HeAac:
                    return new AudioCodec("libfdk_aac", "-profile:a aac_he");
                default:
                    throw new NotSupportedException($"The audio format {format} is not supported.");
            }
        }

        #endregion

        #region Video

        void AppendVideoStream(StringBuilder builder,
                               VideoStreamInfo sourceStream,
                               VideoOutputStream outputStream,
                               string streamSpecifier)
        {
            AppendVideoFormat(builder, streamSpecifier, outputStream.Format);
            AppendDelimiter(builder);
            AppendVideoQuality(builder, outputStream.Quality);

            if (outputStream.DynamicRange == DynamicRange.High)
            {
                if (outputStream.Format != VideoFormat.Hevc)
                {
                    throw new NotSupportedException($"HDR is not supported with the video format {outputStream.Format}.");
                }

                AppendDelimiter(builder);
                AppendVideoHdr(builder);

                if (outputStream.CopyHdrMetadata)
                {
                    if (sourceStream.MasterDisplayProperties != null)
                    {
                        AppendOptionDelimiter(builder);
                        AppendVideoMasterDisplayProperties(builder, sourceStream.MasterDisplayProperties);
                    }

                    if (sourceStream.LightLevelProperties != null)
                    {
                        AppendOptionDelimiter(builder);
                        AppendVideoLightLevelProperties(builder, sourceStream.LightLevelProperties);
                    }
                }
            }

            var filterBuilder = new StringBuilder();

            if ((outputStream.CropParameters != null) &&
                ((outputStream.CropParameters.Size.Width < sourceStream.Dimensions.Width) ||
                 (outputStream.CropParameters.Size.Height < sourceStream.Dimensions.Height)))
            {
                AppendVideoCropFilter(filterBuilder, outputStream.CropParameters);
            }

            if (outputStream.ScaledDimensions.HasValue &&
                !outputStream.ScaledDimensions.Equals(sourceStream.Dimensions))
            {
                AppendListDelimiter(filterBuilder);
                AppendVideoScaleFilter(filterBuilder, outputStream.ScaledDimensions.Value);
            }

            if (outputStream.Tonemap)
            {
                AppendListDelimiter(filterBuilder);
                AppendVideoTonemapFilter(filterBuilder);
            }

            if (filterBuilder.Length > 0)
            {
                AppendDelimiter(builder);
                builder.Append($"-vf {filterBuilder}");
            }
        }

        void AppendVideoFormat(StringBuilder builder, string streamSpecifier, VideoFormat format)
        {
            string codecName = GetVideoCodecName(format);

            builder.Append($"-c:{streamSpecifier} {codecName}");

            VideoCodec config = GetVideoCodecConfig(format);

            if (!string.IsNullOrWhiteSpace(config?.Preset))
            {
                AppendDelimiter(builder);
                builder.Append($"-preset {config.Preset}");
            }
        }

        void AppendVideoQuality(StringBuilder builder, decimal quality)
        {
            builder.Append($"-crf {Math.Round(quality, 2):0.00}");
        }

        void AppendVideoHdr(StringBuilder builder)
        {
            builder.Append("-x265-params");
            AppendDelimiter(builder);
            AppendOption(builder, "colorprim", "bt2020");
            AppendOptionDelimiter(builder);
            AppendOption(builder, "colormatrix", "bt2020nc");
            AppendOptionDelimiter(builder);
            AppendOption(builder, "transfer", "smpte2084");
        }

        void AppendVideoMasterDisplayProperties(StringBuilder builder, MasterDisplayProperties properties)
        {
            AppendOption(builder,
                         "master-display",
                         string.Format("\"G{0}B{1}R{2}WP{3}L{4}\"",
                                       properties.Green,
                                       properties.Blue,
                                       properties.Red,
                                       properties.WhitePoint,
                                       properties.Luminance));
        }

        void AppendVideoLightLevelProperties(StringBuilder builder, LightLevelProperties properties)
        {
            AppendOption(builder, "max-cll", $"\"{properties.MaxCll},{properties.MaxFall}\"");
        }

        void AppendVideoFilterStart(StringBuilder builder, string filter)
        {
            AppendOption(builder, filter, string.Empty);
        }

        void AppendVideoCropFilter(StringBuilder builder, CropParameters cropParameters)
        {
            AppendVideoFilterStart(builder, "crop");
            builder.Append(cropParameters.Size.Width);
            AppendOptionDelimiter(builder);
            builder.Append(cropParameters.Size.Height);
            AppendOptionDelimiter(builder);
            builder.Append(cropParameters.Start.X);
            AppendOptionDelimiter(builder);
            builder.Append(cropParameters.Start.Y);
        }

        void AppendVideoScaleFilter(StringBuilder builder, Dimensions dimensions)
        {
            AppendVideoFilterStart(builder, "scale");
            builder.Append(dimensions.Width);
            AppendOptionDelimiter(builder);
            builder.Append(dimensions.Height);
        }

        void AppendVideoTonemapFilter(StringBuilder builder)
        {
            AppendVideoFilterStart(builder, "zscale");
            AppendOption(builder, "t", "linear");
            AppendOptionDelimiter(builder);
            AppendOption(builder, "npl", "100");
            AppendListDelimiter(builder);
            AppendOption(builder, "format", "gbrpf32le");
            AppendListDelimiter(builder);
            AppendVideoFilterStart(builder, "zscale");
            AppendOption(builder, "p", "bt709");
            AppendListDelimiter(builder);
            AppendOption(builder, "tonemap", "hable");
            AppendOptionDelimiter(builder);
            AppendOption(builder, "desat", "0");
            AppendListDelimiter(builder);
            AppendVideoFilterStart(builder, "zscale");
            AppendOption(builder, "t", "bt709");
            AppendOptionDelimiter(builder);
            AppendOption(builder, "m", "bt709");
            AppendOptionDelimiter(builder);
            AppendOption(builder, "r", "tv");
            AppendListDelimiter(builder);
            AppendOption(builder, "format", "yuv420p");
        }

        string GetVideoCodecName(VideoFormat format)
        {
            switch (format)
            {
                case VideoFormat.Avc:
                    return "libx264";
                case VideoFormat.Hevc:
                    return "libx265";
                default:
                    throw new NotSupportedException($"The video format {format} is not supported.");
            }
        }

        VideoCodec GetVideoCodecConfig(VideoFormat format)
        {
            if ((_config.Video?.Codecs != null) && _config.Video.Codecs.TryGetValue(format, out var codec))
            {
                return codec;
            }

            return new VideoCodec("medium");
        }

        #endregion

        #endregion

        #endregion
    }
}