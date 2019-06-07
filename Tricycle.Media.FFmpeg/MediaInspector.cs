using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            Output output = RunFFprobe(fileName);

            if (output != null)
            {
                result = Map(output);
            }

            return result;
        }

        #endregion

        #region Private

        Output RunFFprobe(string fileName)
        {
            Output result = null;

            using (IProcess process = _processCreator.Invoke())
            {
                var escapedFileName = _processUtility.EscapeFilePath(fileName);
                var startInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    FileName = _ffprobeFileName,
                    Arguments = $"-v quiet -print_format json -show_format -show_streams -i {escapedFileName}",
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
                        result = JsonConvert.DeserializeObject<Output>(builder.ToString());
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

        int GetInt(long? value)
        {
            return value.HasValue ? (int)value : 0;
        }

        #endregion

        #endregion
    }
}
