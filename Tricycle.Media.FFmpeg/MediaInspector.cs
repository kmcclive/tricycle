using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Utilities;
using Tricycle.Media.FFmpeg.Models.FFprobe;
using Tricycle.Models;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg
{
    public class MediaInspector : IMediaInspector
    {
        #region Fields

        readonly string _ffprobeFileName;
        readonly IProcessRunner _processRunner;
        readonly IProcessUtility _processUtility;
        readonly TimeSpan _timeout;

        #endregion

        #region Constructors

        public MediaInspector(string ffprobeFileName,
                              IProcessRunner processRunner,
                              IProcessUtility processUtility)
            : this(ffprobeFileName, processRunner, processUtility, TimeSpan.FromSeconds(5))
        {

        }

        public MediaInspector(string ffprobeFileName,
                              IProcessRunner processRunner,
                              IProcessUtility processUtility,
                              TimeSpan timeout)
        {
            _ffprobeFileName = ffprobeFileName;
            _processRunner = processRunner;
            _processUtility = processUtility;
            _timeout = timeout;
        }

        #endregion

        #region Methods

        #region Public

        public async Task<MediaInfo> Inspect(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException($"{nameof(fileName)} must not be empty or whitespace.", nameof(fileName));
            }

            MediaInfo result = null;
            var output = await RunFFprobe<Output>(fileName, "-show_format -show_streams");

            if (output != null)
            {
                result = Map(fileName, output);
            }

            if (result != null)
            {
                var hdrVideoStreams = result.Streams.OfType<VideoStreamInfo>()
                                                    .Where(s => s.DynamicRange == DynamicRange.High);

                foreach (VideoStreamInfo videoStream in hdrVideoStreams)
                {
                    var options = $"-show_frames -select_streams {videoStream.Index} -read_intervals %+#1";
                    var frameOutput = await RunFFprobe<FrameOutput>(fileName, options);

                    if (frameOutput == null)
                    {
                        continue;
                    }

                    var (displayProperties, lightProperties) = Map(frameOutput);

                    videoStream.MasterDisplayProperties = displayProperties;
                    videoStream.LightLevelProperties = lightProperties;

                    if (displayProperties == null)
                    {
                        // fall back to SDR if metadata is not found
                        videoStream.DynamicRange = DynamicRange.Standard;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Private

        async Task<T> RunFFprobe<T>(string fileName, string options)
        {
            T result = default(T);
            var escapedFileName = _processUtility.EscapeFilePath(fileName);
            var arguments = $"-loglevel error -print_format json {options} -i {escapedFileName}";

            try
            {
                var processResult = await _processRunner.Run(_ffprobeFileName, arguments, _timeout);

                if (!string.IsNullOrWhiteSpace(processResult.OutputData))
                {
                    result = DeserializeOutput<T>(processResult.OutputData, true);
                }
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
            catch (InvalidOperationException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return result;
        }

        T DeserializeOutput<T>(string output, bool retry)
        {
            var result = default(T);

            try
            {
                result = JsonConvert.DeserializeObject<T>(output);
            }
            catch (JsonException ex)
            {
                if (retry)
                {
                    // This is a workaround for when some output is not captured by the process.
                    result = DeserializeOutput<T>(output + "}", false);
                }
                else
                {
                    Trace.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
            }

            return result;
        }

        MediaInfo Map(string fileName, Output output)
        {
            double seconds;

            // ffprobe can identify lots of files that aren't even videos
            // so we are doing some basic validation here
            if (!double.TryParse(output.Format?.Duration, out seconds) ||
                (seconds < 1) ||
                (output.Streams?.Any() != true))
            {
                return null;
            }

            return new MediaInfo()
            {
                FileName = fileName,
                FormatName = output.Format.FormatLongName,
                Duration = TimeSpan.FromSeconds(seconds),
                Streams = output.Streams.Select(s => Map(s)).ToArray()
            };
        }

        StreamInfo Map(Stream stream)
        {
            StreamInfo result = null;

            switch (stream.CodecType)
            {
                case "audio":
                    result = new AudioStreamInfo()
                    {
                        Format = GetAudioFormat(stream.CodecName, stream.Profile),
                        ChannelCount = GetInt(stream.Channels),
                        ProfileName = stream.Profile
                    };
                    break;
                case "subtitle":
                    result = new SubtitleStreamInfo()
                    {
                        SubtitleType = GetSubtitleType(stream.CodecName)
                    };
                    break;
                case "video":
                    var storageDimensions = new Dimensions(GetInt(stream.Width), GetInt(stream.Height));
                    result = new VideoStreamInfo()
                    {
                        BitDepth = GetBitDepth(stream.PixFmt),
                        Dimensions = GetActualDimensions(storageDimensions, stream.SampleAspectRatio),
                        StorageDimensions = storageDimensions,
                        DynamicRange = stream.ColorTransfer == "smpte2084" ? DynamicRange.High : DynamicRange.Standard
                    };
                    break;
                default:
                    result = new StreamInfo();
                    break;
            }

            result.Index = (int)stream.Index;
            result.FormatName = stream.CodecName;

            if (stream.Tags != null)
            {
                result.Language = stream.Tags.TryGetValue("language", out var language) ? language : null;
            }

            return result;
        }

        (MasterDisplayProperties, LightLevelProperties) Map(FrameOutput frameOutput)
        {
            MasterDisplayProperties displayProperties = null;
            LightLevelProperties lightProperties = null;
            Frame frame = frameOutput.Frames?.FirstOrDefault();

            if (frame != null)
            {
                foreach (SideData data in frame.SideDataList)
                {
                    switch (data.SideDataType)
                    {
                        case "Mastering display metadata":
                            displayProperties = new MasterDisplayProperties()
                            {
                                Red = ParseCoordinate(data.RedX, data.RedY),
                                Green = ParseCoordinate(data.GreenX, data.GreenY),
                                Blue = ParseCoordinate(data.BlueX, data.BlueY),
                                WhitePoint = ParseCoordinate(data.WhitePointX, data.WhitePointY),
                                Luminance = ParseRange(data.MinLuminance, data.MaxLuminance)
                            };
                            break;
                        case "Content light level metadata":
                            lightProperties = new LightLevelProperties()
                            {
                                MaxCll = GetInt(data.MaxContent),
                                MaxFall = GetInt(data.MaxAverage)
                            };
                            break;
                    }
                }
            }

            return (displayProperties, lightProperties);
        }

        int GetInt(long? value)
        {
            return value.HasValue ? (int)value : 0;
        }

        int GetBitDepth(string pixelFormat)
        {
            int result = 8;

            if (string.IsNullOrWhiteSpace(pixelFormat))
            {
                return result;
            }

            var match = Regex.Match(pixelFormat, @"^\w+p(?<depth>\d{2})\w+$");

            if (match.Success && int.TryParse(match.Groups["depth"].Value, out var depth))
            {
                result = depth;
            }

            return result;
        }

        SubtitleType GetSubtitleType(string codecName)
        {
            switch (codecName?.ToLower())
            {
                case "dvb_subtitle":
                case "dvd_subtitle":
                case "hdmv_pgs_subtitle":
                case "xsub":
                    return SubtitleType.Graphic;
                default:
                    return SubtitleType.Text;
            }
        }

        AudioFormat? GetAudioFormat(string codecName, string profile)
        {
            if (string.IsNullOrWhiteSpace(codecName))
            {
                return null;
            }

            AudioFormat? result = null;

            if (Regex.IsMatch(codecName, @"aac", RegexOptions.IgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(profile) &&
                    Regex.IsMatch(profile, "HE", RegexOptions.IgnoreCase))
                {
                    result = AudioFormat.HeAac;
                }
                else
                {
                    result = AudioFormat.Aac;
                }
            }
            else if (Regex.IsMatch(codecName, @"ac(\-)?3", RegexOptions.IgnoreCase))
            {
                result = AudioFormat.Ac3;
            }
            else if (Regex.IsMatch(codecName, @"dts", RegexOptions.IgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(profile) &&
                    Regex.IsMatch(profile, "MA", RegexOptions.IgnoreCase))
                {
                    result = AudioFormat.DtsMasterAudio;
                }
                else
                {
                    result = AudioFormat.Dts;
                }
            }
            else if (Regex.IsMatch(codecName, @"true([\s-])?hd", RegexOptions.IgnoreCase))
            {
                result = AudioFormat.DolbyTrueHd;
            }

            return result;
        }

        Dimensions GetActualDimensions(Dimensions storageDimensions, string sampleAspectRatio)
        {
            var (numerator, denominator) = ParseRatio(sampleAspectRatio);
            double ratio = numerator.HasValue && denominator.HasValue ? (double)numerator.Value / denominator.Value : 0;
            int width = storageDimensions.Width;

            if (ratio > 0)
            {
                width = (int)Math.Round(storageDimensions.Width * ratio);
            }

            return new Dimensions(width, storageDimensions.Height);
        }

        Coordinate<int> ParseCoordinate(string xRatio, string yRatio)
        {
            var (x, y) = ParseTuple(xRatio, yRatio);

            return new Coordinate<int>(x, y);
        }

        Range<int> ParseRange(string minRatio, string maxRatio)
        {
            var (min, max) = ParseTuple(minRatio, maxRatio);

            return new Range<int>(min, max);
        }

        (int, int) ParseTuple(string ratio1, string ratio2)
        {
            int? value1 = null;
            int? value2 = null;

            if (ratio1 != null)
            {
                value1 = ParseNumeratorFromRatio(ratio1);
            }

            if (ratio2 != null)
            {
                value2 = ParseNumeratorFromRatio(ratio2);
            }

            return (value1 ?? 0, value2 ?? 0);
        }

        int? ParseNumeratorFromRatio(string ratio)
        {
            var (numerator, _) = ParseRatio(ratio);

            return numerator;
        }

        (int?, int?) ParseRatio(string ratio)
        {
            if (string.IsNullOrWhiteSpace(ratio))
            {
                return (null, null);
            }

            var match = Regex.Match(ratio, @"(?<numerator>\d+)[:/](?<denominator>\d+)");
            int? numerator = null;
            int? denominator = null;

            if (match.Success)
            {
                numerator = int.TryParse(match.Groups["numerator"].Value, out var temp1) ? temp1 : default(int?);
                denominator = int.TryParse(match.Groups["denominator"].Value, out var temp2) ? temp2 : default(int?);
            }

            return (numerator, denominator);
        }

        #endregion

        #endregion
    }
}
