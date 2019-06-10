using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Utilities;
using Tricycle.Media.FFmpeg.Models.FFprobe;
using Tricycle.Media.Models;

namespace Tricycle.Media.FFmpeg
{
    public class MediaInspector : IMediaInspector
    {
        #region Fields

        readonly string _ffprobeFileName;
        readonly Func<IProcess> _processCreator;
        readonly IProcessUtility _processUtility;
        readonly TimeSpan _timeout;

        #endregion

        #region Constructors

        public MediaInspector(string ffprobeFileName, Func<IProcess> processCreator, IProcessUtility processUtility)
            : this(ffprobeFileName, processCreator, processUtility, TimeSpan.FromSeconds(5))
        {

        }

        public MediaInspector(string ffprobeFileName,
                              Func<IProcess> processCreator,
                              IProcessUtility processUtility,
                              TimeSpan timeout)
        {
            _ffprobeFileName = ffprobeFileName;
            _processCreator = processCreator;
            _processUtility = processUtility;
            _timeout = timeout;
        }

        #endregion

        #region Methods

        #region Public

        public MediaInfo Inspect(string fileName)
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
            var output = RunFFprobe<Output>(fileName, "-show_format -show_streams");

            if (output != null)
            {
                result = Map(output);

                var hdrVideoStreams = result.Streams.Where(
                    s => s is VideoStreamInfo && ((VideoStreamInfo)s).DynamicRange == DynamicRange.High);

                foreach (VideoStreamInfo videoStream in hdrVideoStreams)
                {
                    var options = $"-show_frames -select_streams {videoStream.Index} -read_intervals %+#1";
                    var frameOutput = RunFFprobe<FrameOutput>(fileName, options);
                    var (displayProperties, lightProperties) = Map(frameOutput);

                    videoStream.MasterDisplayProperties = displayProperties;
                    videoStream.LightLevelProperties = lightProperties;
                }
            }

            return result;
        }

        #endregion

        #region Private

        T RunFFprobe<T>(string fileName, string options)
        {
            T result = default(T);

            using (IProcess process = _processCreator.Invoke())
            {
                var escapedFileName = _processUtility.EscapeFilePath(fileName);
                var startInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    FileName = _ffprobeFileName,
                    Arguments = $"-loglevel error -print_format json {options} -i {escapedFileName}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var builder = new StringBuilder();
                var completion = new ManualResetEvent(false);

                process.OutputDataReceived += (data) => builder.AppendLine(data);
                process.Exited += () => completion.Set();

                try
                {
                    bool test = process.Start(startInfo);

                    if (!completion.WaitOne(_timeout))
                    {
                        process.Kill();
                    }

                    if (builder.Length > 0)
                    {
                        result = JsonConvert.DeserializeObject<T>(builder.ToString());
                    }
                }
                catch (ArgumentException) { }
                catch (InvalidOperationException) { }
            }

            return result;
        }

        MediaInfo Map(Output output)
        {
            var result = new MediaInfo();

            if (output.Format != null)
            {
                result.FormatName = output.Format.FormatLongName;

                if (double.TryParse(output.Format.Duration, out var seconds))
                {
                    result.Duration = TimeSpan.FromSeconds(seconds);
                }
            }

            if (output.Streams != null)
            {
                result.Streams = output.Streams.Select(s => Map(s)).ToArray();
            }

            return result;
        }

        StreamInfo Map(Stream stream)
        {
            StreamInfo result = null;

            switch (stream.CodecType)
            {
                case "audio":
                    result = new AudioStreamInfo()
                    {
                        ChannelCount = GetInt(stream.Channels)
                    };
                    break;
                case "subtitle":
                    result = new StreamInfo()
                    {
                        StreamType = StreamType.Subtitle
                    };
                    break;
                case "video":
                    result = new VideoStreamInfo()
                    {
                        Dimensions = new Dimensions(GetInt(stream.Width), GetInt(stream.Height)),
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
                value1 = ParseValueFromRatio(ratio1);
            }

            if (ratio2 != null)
            {
                value2 = ParseValueFromRatio(ratio2);
            }

            return (value1 ?? 0, value2 ?? 0);
        }

        int? ParseValueFromRatio(string ratio)
        {
            var match = Regex.Match(ratio, @"(?<value>\d+)/\d+");

            if (match.Success && int.TryParse(match.Groups["value"].Value, out var value))
            {
                return value;
            }

            return null;
        }

        #endregion

        #endregion
    }
}
