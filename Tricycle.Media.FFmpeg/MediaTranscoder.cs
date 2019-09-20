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
        TimeSpan _sourceDuration;
        IProcess _process;
        string _lastError;
        double _avgSpeed;
        long _sampleSize;

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

            if (job.SourceInfo != null)
            {
                _sourceDuration = job.SourceInfo.Duration;
            }
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
            _sourceDuration = TimeSpan.Zero;
        }

        #endregion

        #region Private

        void SubscribeToEvents(IProcess process)
        {
            process.ErrorDataReceived += OnErrorDataReceived;
            process.Exited += OnExited;
        }

        void UnsubscribeFromEvents(IProcess process)
        {
            process.ErrorDataReceived -= OnErrorDataReceived;
            process.Exited -= OnExited;
        }

        void OnErrorDataReceived(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return;
            }

            const string PATTERN =
                @"frame\s*=\s*(?<frame>\d+)\s+fps\s*=\s*(?<fps>\d+(\.\d+)?)\s+q\s*=\s*(?<q>(\-)?\d+(\.\d+)?)\s+" +
                @"size\s*=\s*(?<size>\w+)\s+time\s*=\s*(?<time>\d{2}\:\d{2}\:\d{2}(\.\d+)?)\s+" +
                @"bitrate\s*=\s*(?<bitrate>\d+(.\d+)?\s*\w+/\w)\s+speed\s*=\s*(?<speed>\d+(.\d+)?)x";

            var match = Regex.Match(data, PATTERN, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                if (!Regex.IsMatch(data, @"conversion\s+failed", RegexOptions.IgnoreCase) || (_lastError == null))
                {
                    _lastError = data;
                }
                
                return;
            }

            if (TimeSpan.TryParse(match.Groups["time"].Value, out var time) &&
                double.TryParse(match.Groups["fps"].Value, out var fps) &&
                double.TryParse(match.Groups["speed"].Value, out var speed))
            {
                double percent = 0;
                TimeSpan eta = TimeSpan.Zero;

                if (_sourceDuration > TimeSpan.Zero)
                {
                    percent = time.TotalMilliseconds / _sourceDuration.TotalMilliseconds;

                    if (speed > 0)
                    {
                        eta = CalculateEta(time, _sourceDuration, speed);
                    }
                }

                StatusChanged?.Invoke(new TranscodeStatus()
                {
                    Percent = percent,
                    Time = time,
                    FramesPerSecond = fps,
                    Speed = speed,
                    Size = ParseSize(match.Groups["size"].Value),
                    Eta = eta
                });
            }
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
            _sourceDuration = TimeSpan.Zero;
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

        TimeSpan CalculateEta(TimeSpan timeComplete, TimeSpan totalTime, double speed)
        {
            _sampleSize++;

            if (_avgSpeed > 0)
            {
                _avgSpeed = _avgSpeed + (speed - _avgSpeed) / _sampleSize;
            }
            else
            {
                _avgSpeed = speed;
            }

            return TimeSpan.FromSeconds((totalTime - timeComplete).TotalSeconds / speed);
        }

        #endregion

        #endregion
    }
}
