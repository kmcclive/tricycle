using System;
using System.Collections.Generic;
using System.Linq;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg
{
    public abstract class FFmpegJobRunnerBase
    {
        protected class SubtitleInfo
        {
            public int AbsoluteIndex { get; set; }
            public int RelativeIndex { get; set; }
            public SubtitleType SubtitleType { get; set; }
            public string FileName { get; set; }
        }

        protected IConfigManager<FFmpegConfig> _configManager;
        protected IFFmpegArgumentGenerator _argumentGenerator;

        public FFmpegJobRunnerBase(IConfigManager<FFmpegConfig> configManager,
                                   IFFmpegArgumentGenerator argumentGenerator)
        {
            _configManager = configManager;
            _argumentGenerator = argumentGenerator;
        }

        protected virtual string GenerateArguments(TranscodeJob job)
        {
            var ffmpegJob = Map(job, _configManager.Config);

            return _argumentGenerator.GenerateArguments(ffmpegJob);
        }

        protected virtual FFmpegJob Map(TranscodeJob job, FFmpegConfig config)
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

            var videoSource = job.SourceInfo.Streams.OfType<VideoStreamInfo>().FirstOrDefault();

            if (videoSource == null)
            {
                throw new NotSupportedException($"{nameof(job)}.{nameof(job.SourceInfo)} must contain a video stream.");
            }

            var result = new FFmpegJob()
            {
                HideBanner = true,
                Overwrite = true,
                InputFileName = job.SourceInfo.FileName,
                OutputFileName = job.OutputFileName
            };

            SubtitleInfo subtitleInfo = null;

            if (job.Subtitles != null)
            {
                int i = 0;

                subtitleInfo = job.SourceInfo.Streams.OfType<SubtitleStreamInfo>()
                                                     .Select(s => new SubtitleInfo()
                                                     {
                                                         AbsoluteIndex = s.Index,
                                                         RelativeIndex = i++,
                                                         SubtitleType = s.SubtitleType,
                                                         FileName = job.SourceInfo.FileName
                                                     })
                                                     .FirstOrDefault(s =>
                                                        s.AbsoluteIndex == job.Subtitles.SourceStreamIndex);

                if (subtitleInfo == null)
                {
                    throw new ArgumentException(
                        $"{nameof(job)}.{nameof(job.Subtitles)} contains an invalid index.", nameof(job));
                }

                if (job.Subtitles.ForcedOnly)
                {
                    result.ForcedSubtitlesOnly = true;
                }

                result.CanvasSize = videoSource.Dimensions;
            }

            result.Streams = MapStreams(config, job);

            var videoOutput = job.Streams.OfType<VideoOutputStream>()
                                         .FirstOrDefault(s => s.SourceStreamIndex == videoSource.Index);

            if (videoOutput != null)
            {
                result.Filters = GetVideoFilters(config, videoSource, videoOutput, subtitleInfo);
            }

            return result;
        }

        protected virtual IList<MappedStream> MapStreams(FFmpegConfig config, TranscodeJob job)
        {
            IList<MappedStream> result = new List<MappedStream>();
            IDictionary<int, StreamInfo> sourceStreamsByIndex = job.SourceInfo.Streams.ToDictionary(s => s.Index);

            for (int i = 0; i < job.Streams.Count; i++)
            {
                OutputStream outputStream = job.Streams[i];
                StreamInfo sourceStream;

                if (!sourceStreamsByIndex.TryGetValue(outputStream.SourceStreamIndex, out sourceStream))
                {
                    throw new ArgumentException(
                        $"{nameof(job.SourceInfo)} does not contain a stream with index {outputStream.SourceStreamIndex}.",
                        nameof(job.SourceInfo));
                }

                MappedStream mappedStream = MapStream(config, sourceStream, outputStream);

                if (mappedStream != null)
                {
                    result.Add(mappedStream);
                }
            }

            return result;
        }

        protected virtual MappedStream MapStream(FFmpegConfig config,
                                                 StreamInfo sourceStream,
                                                 OutputStream outputStream)
        {
            if (sourceStream is VideoStreamInfo videoSource)
            {
                if (outputStream is VideoOutputStream videoOutput)
                {
                    return MapVideoStream(config, videoSource, videoOutput);
                }

                throw GetStreamMismatchException(nameof(sourceStream), nameof(outputStream));
            }

            return null;
        }

        protected virtual Exception GetStreamMismatchException(string sourceStreamName, string outputStreamName)
        {
            return new ArgumentException($"{sourceStreamName} and {outputStreamName} types do not match.",
                                         outputStreamName);
        }

        protected virtual MappedVideoStream MapVideoStream(FFmpegConfig config,
                                                           VideoStreamInfo sourceStream,
                                                           VideoOutputStream outputStream)
        {
            return new MappedVideoStream()
            {
                Input = GetStreamInput(sourceStream),
            };
        }

        protected virtual StreamInput GetStreamInput(StreamInfo stream)
        {
            return new StreamInput(0, stream.Index);
        }

        protected virtual IList<IFilter> GetVideoFilters(FFmpegConfig config,
                                                         VideoStreamInfo sourceStream,
                                                         VideoOutputStream outputStream,
                                                         SubtitleInfo subtitleInfo)
        {
            var result = new List<IFilter>();

            if (subtitleInfo != null)
            {
                if (subtitleInfo.SubtitleType == SubtitleType.Graphic)
                {
                    const string SUB_LABEL = "sub";
                    const string REF_LABEL = "ref";

                    result.Add(GetScale2RefFilter(sourceStream, subtitleInfo.AbsoluteIndex, SUB_LABEL, REF_LABEL));
                    result.Add(GetOverlayFilter(REF_LABEL, SUB_LABEL));
                }
                else
                {
                    result.Add(GetSubtitlesFilter(subtitleInfo));
                }
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

        protected virtual IFilter GetScale2RefFilter(VideoStreamInfo sourceStream,
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

        protected virtual IFilter GetSubtitlesFilter(SubtitleInfo subtitleInfo)
        {
            return new Filter("subtitles")
            {
                Options = new Option[]
                {
                    Option.FromValue($"\"{subtitleInfo.FileName}\""),
                    new Option("si", subtitleInfo.RelativeIndex.ToString())
                }
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
