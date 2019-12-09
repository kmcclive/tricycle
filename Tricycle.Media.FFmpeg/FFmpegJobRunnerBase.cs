using System;
using System.Collections.Generic;
using System.Linq;
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
        IFFmpegArgumentGenerator _argumentGenerator;

        public FFmpegJobRunnerBase(IConfigManager<FFmpegConfig> configManager, IFFmpegArgumentGenerator argumentGenerator)
        {
            _configManager = configManager;
            _argumentGenerator = argumentGenerator;
        }

        protected virtual string GenerateArguments(TranscodeJob job, JobType jobType)
        {
            var ffmpegJob = Map(job, jobType, _configManager.Config);

            return _argumentGenerator.GenerateArguments(ffmpegJob);
        }

        protected virtual FFmpegJob Map(TranscodeJob job, JobType jobType, FFmpegConfig config)
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
                MapVideoStream(config, videoSourceStream, videoOutputStream, jobType)
            };

            result.Filters = GetVideoFilters(config, videoSourceStream, videoOutputStream, subtitlesIndex);

            return result;
        }

        protected virtual MappedVideoStream MapVideoStream(FFmpegConfig config,
                                                           VideoStreamInfo sourceStream,
                                                           VideoOutputStream outputStream,
                                                           JobType jobType)
        {
            var result = new MappedVideoStream()
            {
                Input = GetStreamInput(sourceStream),
            };

            if (jobType == JobType.Transcode)
            {
                result.Codec = GetVideoCodec(config, sourceStream, outputStream);
            }

            return result;
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
            X26xCodec result = format == VideoFormat.Hevc ? new X265Codec(codecName) : new X26xCodec(codecName);

            result.Preset = codec.Preset;
            result.Crf = outputStream.Quality;

            if (outputStream.DynamicRange == DynamicRange.High)
            {
                if (outputStream.Format != VideoFormat.Hevc)
                {
                    throw new NotSupportedException($"HDR is not supported with the video format {outputStream.Format}.");
                }

                var options = new List<Option>()
                {
                    new Option("colorprim", "bt2020"),
                    new Option("colormatrix", "bt2020nc"),
                    new Option("transfer", "smpte2084")
                };

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

                        options.Add(new Option("master-display", value));
                    }

                    if (sourceStream.LightLevelProperties != null)
                    {
                        var properties = sourceStream.LightLevelProperties;

                        options.Add(new Option("max-cli", $"\"{properties.MaxCll},{properties.MaxFall}\""));
                    }
                }

                ((X265Codec)result).Options = options;
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

        protected virtual IList<IFilter> GetVideoFilters(FFmpegConfig config,
                                                         VideoStreamInfo sourceStream,
                                                         VideoOutputStream outputStream,
                                                         int? subtitlesIndex)
        {
            var result = new List<IFilter>();

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

            if (outputStream.Denoise)
            {
                result.Add(GetDenoiseFilter(config));
            }

            if (outputStream.Tonemap)
            {
                AddTonemapFilters(result, config);
            }

            return result;
        }

        protected virtual IFilter GetSub2RefFilter(VideoStreamInfo sourceStream,
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

        protected virtual IFilter GetOverlayFilter(string bottomLabel, string topLabel)
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

        protected virtual IFilter GetCropFilter(CropParameters parameters)
        {
            return new Filter("crop")
            {
                Options = new Option[]
                {
                    Option.FromValue(parameters.Size.Width),
                    Option.FromValue(parameters.Size.Height),
                    Option.FromValue(parameters.Start.X),
                    Option.FromValue(parameters.Start.Y)
                }
            };
        }

        protected virtual IFilter GetScaleFilter(Dimensions dimensions)
        {
            return new Filter("scale")
            {
                Options = new Option[]
                {
                    Option.FromValue(dimensions.Width),
                    Option.FromValue(dimensions.Height)
                }
            };
        }

        protected virtual IFilter GetSampleAspectRatioFilter(int width, int height)
        {
            return new Filter("setsar")
            {
                Options = new Option[]
                {
                    Option.FromValue(width),
                    Option.FromValue(height)
                }
            };
        }

        protected virtual IFilter GetDenoiseFilter(FFmpegConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config?.Video?.DenoiseOptions))
            {
                return new CustomFilter(config.Video.DenoiseOptions);
            }

            return new Filter("hqdn3d")
            {
                Options = new Option[]
                {
                    Option.FromValue(4),
                    Option.FromValue(4),
                    Option.FromValue(3),
                    Option.FromValue(3)
                }
            };
        }

        protected virtual void AddTonemapFilters(IList<IFilter> filters, FFmpegConfig config)
        {
            filters.Add(new Filter("zscale")
            {
                Options = new Option[]
                {
                    new Option("t", "linear"),
                    new Option("npl", "100")
                }
            });
            filters.Add(new Filter("format")
            {
                Options = new Option[]
                {
                    Option.FromValue("gbrpf32le")
                }
            });
            filters.Add(new Filter("zscale")
            {
                Options = new Option[]
                {
                    new Option("p", "bt709")
                }
            });
            filters.Add(GetTonemapFilter(config));
            filters.Add(new Filter("zscale")
            {
                Options = new Option[]
                {
                    new Option("t", "bt709"),
                    new Option("m", "bt709"),
                    new Option("r", "tv")
                }
            });
            filters.Add(new Filter("format")
            {
                Options = new Option[]
                {
                    Option.FromValue("yuv420p")
                }
            });
        }

        protected virtual IFilter GetTonemapFilter(FFmpegConfig config)
        {
            const string NAME = "tonemap";

            if (!string.IsNullOrWhiteSpace(config?.Video?.TonemapOptions))
            {
                return new CustomFilter($"{NAME}={config.Video.TonemapOptions}");
            }

            return new Filter(NAME)
            {
                Options = new Option[]
                {
                    Option.FromValue("hable"),
                    new Option("desat", "0")
                }
            };
        }
    }
}
