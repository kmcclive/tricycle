using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Tricycle.Diagnostics;
using Tricycle.Models.Jobs;

namespace Tricycle.Media.FFmpeg
{
    public class MediaTranscoder : IMediaTranscoder
    {
        readonly string _ffmpegFileName;
        readonly Func<IProcess> _processCreator;
        readonly IFFmpegArgumentGenerator _argumentGenerator;
        IProcess _process;
        string _lastError;

        public MediaTranscoder(string ffmpegFileName,
                               Func<IProcess> processCreator,
                               IFFmpegArgumentGenerator argumentGenerator)
        {
            _ffmpegFileName = ffmpegFileName;
            _processCreator = processCreator;
            _argumentGenerator = argumentGenerator;
        }

        public bool IsRunning => _process != null && !_process.HasExited;

        public event Action<TranscodeStatus> StatusChanged;
        public event Action Completed;
        public event Action<string> Failed;

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
        }

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
                    Speed = speed
                });
            }
        }

        void OnErrorDataReceived(string data)
        {
            _lastError = data;
        }

        private void OnExited()
        {
            if (_process.ExitCode == 0)
            {
                Completed?.Invoke();
            }
            else
            {
                Failed?.Invoke(_lastError);
            }
        }
    }
}
