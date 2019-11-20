using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics.Utilities;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models;
using Tricycle.Models;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class FFmpegArgumentGeneratorTests
    {
        #region Fields

        IProcessUtility _processUtility;
        FFmpegConfig _config;
        IConfigManager<FFmpegConfig> _configManager;
        FFmpegArgumentGenerator _generator;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void Setup()
        {
            _processUtility = Substitute.For<IProcessUtility>();
            _config = new FFmpegConfig();
            _configManager = Substitute.For<IConfigManager<FFmpegConfig>>();
            _configManager.Config = _config;
            _generator = new FFmpegArgumentGenerator(_processUtility, _configManager);

            _processUtility.EscapeFilePath(Arg.Any<string>()).Returns(x => $"\"{x[0]}\"");
        }

        #endregion

        #region Test Methods

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateArgumentsThrowsForNullArgument()
        {
            _generator.GenerateArguments(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateArgumentsThrowsForNullSourceInfo()
        {
            var job = CreateDefaultJob();

            job.SourceInfo = null;

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateArgumentsThrowsForEmptyInputFileName()
        {
            var job = CreateDefaultJob();

            job.SourceInfo.FileName = string.Empty;

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateArgumentsThrowsForEmptyOutputFileName()
        {
            var job = CreateDefaultJob();

            job.OutputFileName = string.Empty;

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateArgumentsThrowsForEmptyInputStreams()
        {
            var job = CreateDefaultJob();

            job.SourceInfo.Streams = new StreamInfo[0];

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateArgumentsThrowsForEmptyOutputStreams()
        {
            var job = CreateDefaultJob();

            job.Streams = new OutputStream[0];

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateArgumentsThrowsForNoSourceVideoStream()
        {
            var job = CreateDefaultJob();

            job.Streams = new OutputStream[0];

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateArgumentsThrowsForMissingInputStream()
        {
            var job = CreateDefaultJob();

            job.Streams[0].SourceStreamIndex = 2;

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateArgumentsThrowsForStreamTypeMismatch()
        {
            var job = CreateDefaultJob();

            job.SourceInfo.Streams = new StreamInfo[]
            {
                new VideoStreamInfo()
                {
                    Index = 0
                },
                new AudioStreamInfo()
                {
                    Index = 1
                }
            };
            job.Streams = new OutputStream[]
            {
                new VideoOutputStream()
                {
                    SourceStreamIndex = 2
                }
            };

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void GenerateArgumentsThrowsForUnsupportedHdrFormat()
        {
            var job = CreateDefaultJob();

            job.Streams = new OutputStream[]
            {
                new VideoOutputStream()
                {
                    SourceStreamIndex = 0,
                    Format = VideoFormat.Avc,
                    DynamicRange = DynamicRange.High
                }
            };

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        public void GeneratesArgumentsForHdrJob()
        {
            var job = new TranscodeJob()
            {
                Format = ContainerFormat.Mkv,
                OutputFileName = "output.mkv",
                SourceInfo = new MediaInfo()
                {
                    FileName = "input.mkv",
                    Streams = new StreamInfo[]
                    {
                        new VideoStreamInfo()
                        {
                            Index = 0,
                            Dimensions = new Dimensions(3840, 2160),
                            DynamicRange = DynamicRange.High,
                            MasterDisplayProperties = new MasterDisplayProperties()
                            {
                                Red = new Coordinate<int>(34000, 16000),
                                Green = new Coordinate<int>(13250, 34500),
                                Blue = new Coordinate<int>(7500, 3000),
                                WhitePoint = new Coordinate<int>(15635, 16450),
                                Luminance = new Range<int>(1, 10000000)
                            },
                            LightLevelProperties = new LightLevelProperties()
                            {
                                MaxCll = 1000,
                                MaxFall = 400
                            }
                        },
                        new AudioStreamInfo()
                        {
                            Index = 1,
                            ChannelCount = 8
                        }
                    }
                },
                Streams = new OutputStream[]
                {
                    new VideoOutputStream()
                    {
                        SourceStreamIndex = 0,
                        Format = VideoFormat.Hevc,
                        DynamicRange = DynamicRange.High,
                        Quality = 18,
                        CropParameters = new CropParameters()
                        {
                            Size = new Dimensions(3840, 1632),
                            Start = new Coordinate<int>(0, 264)
                        },
                        CopyHdrMetadata = true,
                        Denoise = true,
                    },
                    new OutputStream()
                    {
                        SourceStreamIndex = 1
                    },
                    new AudioOutputStream()
                    {
                        SourceStreamIndex = 1,
                        Format = AudioFormat.HeAac,
                        Quality = 80,
                        Mixdown = AudioMixdown.Stereo
                    }
                }
            };

            var actual = _generator.GenerateArguments(job);
            string expected = "-hide_banner -y -i \"input.mkv\" -f matroska " +
                "-map 0:0 -c:v:0 libx265 -preset medium -crf 18 -x265-params " +
                "colorprim=bt2020:colormatrix=bt2020nc:transfer=smpte2084:" +
                "master-display=\"G(13250,34500)B(7500,3000)R(34000,16000)WP(15635,16450)L(10000000,1)\":max-cll=\"1000,400\" " +
                "-filter_complex [0:0]crop=3840:1632:0:264,setsar=1:1,hqdn3d=4:4:3:3 " +
                "-map 0:1 -c:a:0 copy " +
                "-map 0:1 -c:a:1 libfdk_aac -profile:a aac_he -ac:a:1 2 -b:a:1 80k " +
                "\"output.mkv\"";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GeneratesArgumentsForTonemapJob()
        {
            var job = new TranscodeJob()
            {
                Format = ContainerFormat.Mp4,
                OutputFileName = "output.m4v",
                SourceInfo = new MediaInfo()
                {
                    FileName = "input.mkv",
                    Streams = new StreamInfo[]
                    {
                        new VideoStreamInfo()
                        {
                            Index = 0,
                            Dimensions = new Dimensions(3840, 2160),
                            DynamicRange = DynamicRange.High,
                            MasterDisplayProperties = new MasterDisplayProperties(),
                            LightLevelProperties = new LightLevelProperties()
                        },
                        new AudioStreamInfo()
                        {
                            Index = 1,
                            ChannelCount = 6
                        },
                        new StreamInfo()
                        {
                            Index = 2,
                            StreamType = StreamType.Subtitle
                        },
                        new StreamInfo()
                        {
                            Index = 3,
                            StreamType = StreamType.Subtitle
                        }
                    }
                },
                Streams = new OutputStream[]
                {
                    new VideoOutputStream()
                    {
                        SourceStreamIndex = 0,
                        Format = VideoFormat.Avc,
                        DynamicRange = DynamicRange.Standard,
                        Quality = 20 + 1/3M,
                        ScaledDimensions = new Dimensions(1920, 1080),
                        Tonemap = true
                    },
                    new AudioOutputStream()
                    {
                        SourceStreamIndex = 1,
                        Format = AudioFormat.Aac,
                        Quality = 160,
                        Mixdown = AudioMixdown.Stereo
                    },
                    new AudioOutputStream()
                    {
                        SourceStreamIndex = 1,
                        Format = AudioFormat.Ac3,
                        Quality = 640,
                        Mixdown = AudioMixdown.Surround5dot1
                    },
                },
                Subtitles = new SubtitlesConfig()
                {
                    SourceStreamIndex = 3,
                    ForcedOnly = true
                }
            };

            var actual = _generator.GenerateArguments(job);
            string expected = "-hide_banner -y -forced_subs_only 1 -canvas_size 3840x2160 -i \"input.mkv\" -f mp4 " +
                "-map 0:0 -c:v:0 libx264 -preset medium -crf 20.33 " +
                "-filter_complex [0:3][0:0]'scale2ref[sub][ref];[ref][sub]overlay',scale=1920:1080,setsar=1:1," +
                "zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709," +
                "tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv,format=yuv420p " +
                "-map 0:1 -c:a:0 aac -ac:a:0 2 -b:a:0 160k " +
                "-map 0:1 -c:a:1 ac3 -ac:a:1 6 -b:a:1 640k " +
                "\"output.m4v\"";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GeneratesArgumentsForMkvSubtitleJob()
        {
            var job = new TranscodeJob()
            {
                Format = ContainerFormat.Mkv,
                OutputFileName = "output.mkv",
                SourceInfo = new MediaInfo()
                {
                    FileName = "input.m4v",
                    Duration = new TimeSpan(0, 1, 42, 37, 610),
                    Streams = new StreamInfo[]
                    {
                        new VideoStreamInfo()
                        {
                            Index = 0,
                            Dimensions = new Dimensions(3840, 2160),
                            DynamicRange = DynamicRange.Standard
                        },
                        new StreamInfo()
                        {
                            Index = 1,
                            StreamType = StreamType.Subtitle
                        }
                    }
                },
                Streams = new OutputStream[]
                {
                    new VideoOutputStream()
                    {
                        SourceStreamIndex = 0,
                        Format = VideoFormat.Avc,
                        DynamicRange = DynamicRange.Standard,
                        Quality = 20
                    }
                },
                Subtitles = new SubtitlesConfig()
                {
                    SourceStreamIndex = 1
                }
            };

            var actual = _generator.GenerateArguments(job);
            string expected = "-hide_banner -y -canvas_size 3840x2160 -i \"input.m4v\" -f matroska -t 1:42:37.610 " +
                "-map 0:0 -c:v:0 libx264 -preset medium -crf 20 " +
                "-filter_complex [0:1][0:0]'scale2ref[sub][ref];[ref][sub]overlay' " +
                "\"output.mkv\"";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GenerateArgumentsRespectsConfig()
        {
            _config.Video = new VideoConfig()
            {
                Codecs = new Dictionary<VideoFormat, VideoCodec>()
                {
                    { VideoFormat.Avc, new VideoCodec("slow") }
                }
            };
            _config.Audio = new AudioConfig()
            {
                Codecs = new Dictionary<AudioFormat, AudioCodec>()
                {
                    { AudioFormat.Aac, new AudioCodec("libfdk_aac", "-profile:a aac_low") }
                }
            };

            var job = new TranscodeJob()
            {
                Format = ContainerFormat.Mp4,
                OutputFileName = "output.m4v",
                SourceInfo = new MediaInfo()
                {
                    FileName = "input.mkv",
                    Streams = new StreamInfo[]
                    {
                        new VideoStreamInfo()
                        {
                            Index = 0,
                            Dimensions = new Dimensions(1920, 1080)
                        },
                        new AudioStreamInfo()
                        {
                            Index = 1,
                            ChannelCount = 6
                        }
                    }
                },
                Streams = new OutputStream[]
                {
                    new VideoOutputStream()
                    {
                        SourceStreamIndex = 0,
                        Format = VideoFormat.Avc,
                        Quality = 22
                    },
                    new AudioOutputStream()
                    {
                        SourceStreamIndex = 1,
                        Format = AudioFormat.Aac,
                        Quality = 80,
                        Mixdown = AudioMixdown.Stereo
                    }
                }
            };

            var actual = _generator.GenerateArguments(job);
            string expected = "-hide_banner -y -i \"input.mkv\" -f mp4 " +
                "-map 0:0 -c:v:0 libx264 -preset slow -crf 22 " +
                "-map 0:1 -c:a:0 libfdk_aac -profile:a aac_low -ac:a:0 2 -b:a:0 80k " +
                "\"output.m4v\"";

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Helper Methods

        TranscodeJob CreateDefaultJob()
        {
            return new TranscodeJob()
            {
                SourceInfo = new MediaInfo()
                {
                    FileName = "input.mkv",
                    Streams = new StreamInfo[]
                    {
                        new VideoStreamInfo()
                        {
                            Index = 0
                        }
                    }
                },
                OutputFileName = "output.mkv",
                Streams = new OutputStream[]
                {
                    new OutputStream()
                    {
                        SourceStreamIndex = 0
                    }
                }
            };
        }

        #endregion
    }
}
