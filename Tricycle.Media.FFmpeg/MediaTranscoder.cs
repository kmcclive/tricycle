using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Tricycle.Diagnostics;
using Tricycle.Models.Jobs;

namespace Tricycle.Media.FFmpeg
{
    public class MediaTranscoder : IMediaTranscoder
    {
        #region Fields

        readonly string _ffmpegFileName;
        readonly Func<IProcess> _processCreator;
        readonly IFFmpegArgumentGenerator _argumentGenerator;
        IProcess _process;
        string _lastError;

        #endregion

        #region Constructors

        public MediaTranscoder(string ffmpegFileName,
                               Func<IProcess> processCreator,
                               IFFmpegArgumentGenerator argumentGenerator)
        {
            _ffmpegFileName = ffmpegFileName;
            _processCreator = processCreator;
            _argumentGenerator = argumentGenerator;
        }

        #endregion

        #region Properties

        public bool IsRunning => _process != null && !_process.HasExited;

        #endregion

        #region Events

        public event Action<TranscodeStatus> StatusChanged;
        public event Action Completed;
        public event Action<string> Failed;

        #endregion

        #region Methods

        #region Public

        public void Start(TranscodeJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (IsRunning)
            {
                throw new InvalidOperationException("A job is already running.");
            }

            string arguments = _argumentGenerator.GenerateArguments(job);
            var startInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = _ffmpegFileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            _process = _processCreator.Invoke();

            SubscribeToEvents(_process);

            _process.Start(startInfo);
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("No job is running.");
            }

            UnsubscribeFromEvents(_process);

            _process.Kill();
            _process.Dispose();

            _process = null;
        }

        #endregion

        #region Private

        void SubscribeToEvents(IProcess process)
        {
            process.OutputDataReceived += OnOutputDataReceived;
            process.ErrorDataReceived += OnErrorDataReceived;
            process.Exited += OnExited;
        }

        void UnsubscribeFromEvents(IProcess process)
        {
            process.OutputDataReceived -= OnOutputDataReceived;
            process.ErrorDataReceived -= OnErrorDataReceived;
            process.Exited -= OnExited;
        }

        void OnOutputDataReceived(string data)
        {
            const string PATTERN =
                @"frame\s*=\s*(?<frame>\d+)\s+fps\s*=\s*(?<fps>\d+(\.\d+)?)\s+q\s*=\s*(?<q>\-\d+(\.\d+)?)\s+" +
                @"size\s*=\s*(?<size>\w+)\s+time\s*=\s*(?<time>\d{2}\:\d{2}\:\d{2}(\.\d{2})?)\s+" +
                @"bitrate=(?<bitrate>\d+(.\d+)?\s*\w+/\w)\s+dup\s*=\s*(?<dup>\d+)\s+drop\s*=\s*(?<drop>\d+)\s+" +
                @"speed\s*=\s*(?<speed>\d+(.\d+)?)x";

            var match = Regex.Match(data, PATTERN, RegexOptions.IgnoreCase);

            if (match.Success &&
                TimeSpan.TryParse(match.Groups["time"].Value, out var time) &&
                double.TryParse(match.Groups["fps"].Value, out var fps) &&
                double.TryParse(match.Groups["speed"].Value, out var speed))
            {
                StatusChanged?.Invoke(new TranscodeStatus()
                {
                    Time = time,
                    FramesPerSecond = fps,
                    Speed = speed,
                    Size = ParseSize(match.Groups["size"].Value)
                });
            }
        }

        void OnErrorDataReceived(string data)
        {
            _lastError = data;
        }

        void OnExited()
        {
            // This is a workaround for a bug in the .NET code.
            // See https://stackoverflow.com/a/25772586/9090758 for more details.
            _process.WaitForExit();

            if (_process.ExitCode == 0)
            {
                Completed?.Invoke();
            }
            else
            {
                Failed?.Invoke(_lastError);
            }
  
            _process.Dispose();

            _process = null;
        }

        long ParseSize(string size)
        {
            long result = 0;

            if (!string.IsNullOrWhiteSpace(size))
            {
                var match = Regex.Match(size, @"(?<amount>\d+(\.\d+)?)(?<unit>\w+)");

                if (match.Success &&
                    double.TryParse(match.Groups["amount"].Value, out var amount))
                {
                    string unit = match.Groups["unit"].Value;
                    int factor = 1;

                    switch (unit?.ToLower())
                    {
                        case "kb":
                            factor = 1000;
                            break;
                        case "mb":
                            factor = 1000000;
                            break;
                        case "gb":
                            factor = 1000000000;
                            break;
                    }

                    result = (long)Math.Round(amount * factor);
                }
            }

            return result;
        }

        #endregion

        #endregion
    }
}
