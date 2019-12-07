using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;
using Tricycle.Utilities;

namespace Tricycle.Media.FFmpeg
{
    public abstract class FFmpegJobRunnerBase
    {
        protected enum JobType
        {
            Transcode,
            Preview
        }

        IConfigManager<FFmpegConfig> _configManager;

        public FFmpegJobRunnerBase(IConfigManager<FFmpegConfig> configManager)
        {
            _configManager = configManager;
        }

        protected virtual FFmpegJob Map(TranscodeJob job, JobType jobType)
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

            var videoSourceStream = job.SourceInfo.Streams.OfType<VideoStreamInfo>().FirstOrDefault();

            if (videoSourceStream == null)
            {
                throw new NotSupportedException($"{nameof(job)}.{nameof(job.SourceInfo)} must contain a video stream.");
            }

            var videoOutputStream = job.Streams.OfType<VideoOutputStream>().FirstOrDefault();

            if (videoOutputStream == null)
            {
                throw new NotSupportedException($"{nameof(job)}.{nameof(job.Streams)} must contain a video stream.");
            }

            var result = new FFmpegJob()
            {
                HideBanner = true,
                Overwrite = true,
                InputFileName = job.SourceInfo.FileName,
                OutputFileName = job.OutputFileName
            };

            int? subtitlesIndex = null;

            if (job.Subtitles != null)
            {
                if (!job.SourceInfo.Streams.Any(s => s.StreamType == StreamType.Subtitle))
                {
                    throw new ArgumentException(
                        $"{nameof(job)}.{nameof(job.Subtitles)} contains an invalid index.", nameof(job));
                }

                subtitlesIndex = job.Subtitles.SourceStreamIndex;

                if (job.Subtitles.ForcedOnly)
                {
                    result.ForcedSubtitlesOnly = true;
                }

                result.CanvasSize = videoSourceStream.Dimensions;
            }

            result.Streams = new List<MappedStream>()
            {
                Map(_configManager.Config, videoSourceStream, videoOutputStream)
            };

            result.Filters = GetVideoFilters(_configManager.Config, videoSourceStream, videoOutputStream, subtitlesIndex);

            return result;
        }

        protected virtual MappedVideoStream Map(FFmpegConfig config,
                                                VideoStreamInfo sourceStream,
                                                VideoOutputStream outputStream)
        {
            return new MappedVideoStream()
            {
                Input = GetStreamInput(sourceStream),
                Codec = GetVideoCodec(config, sourceStream, outputStream)
            };
        }

        protected virtual StreamInput GetStreamInput(StreamInfo stream)
        {
            return new StreamInput(0, stream.Index);
        }

        protected virtual Codec GetVideoCodec(FFmpegConfig config,
                                              VideoStreamInfo sourceStream,
                                              VideoOutputStream outputStream)
        {
            VideoFormat format = outputStream.Format;
            VideoCodec codec = config.Video?.Codecs.GetValueOrDefault(format) ?? new VideoCodec("medium");
            string codecName = GetVideoCodecName(format);

            var result = new Codec(codecName)
            {
                Options = new Dictionary<string, string>()
                {
                    { "preset", codec.Preset },
                    { "crf", outputStream.Quality.ToString("0.##") }
                }
            };

            if (outputStream.DynamicRange == DynamicRange.High)
            {
                if (outputStream.Format != VideoFormat.Hevc)
                {
                    throw new NotSupportedException($"HDR is not supported with the video format {outputStream.Format}.");
                }

                var optionBuilder = new StringBuilder("colorprim=bt2020:colormatrix=bt2020nc:transfer=smpte2084");

                if (outputStream.CopyHdrMetadata)
                {
                    if (sourceStream.MasterDisplayProperties != null)
                    {
                        var properties = sourceStream.MasterDisplayProperties;
                        var value = string.Format("\"G{0}B{1}R{2}WP{3}L({4},{5})\"",
                                                  properties.Green,
                                                  properties.Blue,
                                                  properties.Red,
                                                  properties.WhitePoint,
                                                  properties.Luminance.Max,
                                                  properties.Luminance.Min);

                        optionBuilder.Append($":master-display={value}");
                    }

                    if (sourceStream.LightLevelProperties != null)
                    {
                        var properties = sourceStream.LightLevelProperties;

                        optionBuilder.Append($":max-cli=\"{properties.MaxCll},{properties.MaxFall}\"");
                    }
                }

                result.Options["x265-params"] = optionBuilder.ToString();
            }

            return result;
        }

        protected virtual string GetVideoCodecName(VideoFormat format)
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

        protected virtual IList<Filter> GetVideoFilters(FFmpegConfig config,
                                                        VideoStreamInfo sourceStream,
                                                        VideoOutputStream outputStream,
                                                        int? subtitlesIndex)
        {
            var result = new List<Filter>();

            if (subtitlesIndex.HasValue)
            {
                const string SUB_LABEL = "sub";
                const string REF_LABEL = "ref";

                result.Add(GetSub2RefFilter(sourceStream, subtitlesIndex, SUB_LABEL, REF_LABEL));
                result.Add(GetOverlayFilter(REF_LABEL, SUB_LABEL));
            }

            bool setSampleAspectRatio = false;

            if ((outputStream.CropParameters != null) &&
                ((outputStream.CropParameters.Size.Width < sourceStream.Dimensions.Width) ||
                 (outputStream.CropParameters.Size.Height < sourceStream.Dimensions.Height)))
            {
                result.Add(GetCropFilter(outputStream.CropParameters));

                setSampleAspectRatio = true;
            }

            if (outputStream.ScaledDimensions.HasValue &&
                !outputStream.ScaledDimensions.Equals(sourceStream.Dimensions))
            {
                result.Add(GetScaleFilter(outputStream.ScaledDimensions.Value));

                setSampleAspectRatio = true;
            }

            if (setSampleAspectRatio)
            {
                result.Add(GetSampleAspectRatioFilter(1, 1));
            }

            return result;
        }

        protected virtual Filter GetSub2RefFilter(VideoStreamInfo sourceStream,
                                                  int? subtitlesIndex,
                                                  string subLabel,
                                                  string refLabel)
        {
            return new Filter("scale2ref")
            {
                Name = "scale2ref",
                Inputs = new IInput[]
                {
                    new StreamInput(0, subtitlesIndex.Value),
                    new StreamInput(0, sourceStream.Index)
                },
                OutputLabels = new string[]
                {
                    subLabel,
                    refLabel
                }
            };
        }

        protected virtual Filter GetOverlayFilter(string bottomLabel, string topLabel)
        {
            return new Filter("overlay")
            {
                Inputs = new IInput[]
                {
                    new LabeledInput(bottomLabel),
                    new LabeledInput(topLabel)
                },
                ChainToPrevious = true
            };
        }

        protected virtual Filter GetCropFilter(CropParameters parameters)
        {
            return new Filter("crop")
            {
                Options = new FilterOption[]
                {
                    FilterOption.FromValue(parameters.Size.Width),
                    FilterOption.FromValue(parameters.Size.Height),
                    FilterOption.FromValue(parameters.Start.X),
                    FilterOption.FromValue(parameters.Start.Y)
                }
            };
        }

        protected virtual Filter GetScaleFilter(Dimensions dimensions)
        {
            return new Filter("scale")
            {
                Options = new FilterOption[]
                {
                    FilterOption.FromValue(dimensions.Width),
                    FilterOption.FromValue(dimensions.Height)
                }
            };
        }

        protected virtual Filter GetSampleAspectRatioFilter(int width, int height)
        {
            return new Filter("setsar")
            {
                Options = new FilterOption[]
                {
                    FilterOption.FromValue(width),
                    FilterOption.FromValue(height)
                }
            };
        }
    }
}
