using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tricycle.Diagnostics;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg
{
    public class InterlaceDetector : IInterlaceDetector
    {
        class FrameStatistics
        {
            public int TffCount { get; set; }
            public int BffCount { get; set; }
            public int ProgressiveCount { get; set; }
            public int UndeterminedCount { get; set; }
        }

        const int FRAME_COUNT = 100;

        readonly string _ffmpegFileName;
        readonly IProcessRunner _processRunner;
        readonly IFFmpegArgumentGenerator _argumentGenerator;
        readonly TimeSpan _timeout;

        public InterlaceDetector(string ffmpegFileName,
                                 IProcessRunner processRunner,
                                 IFFmpegArgumentGenerator argumentGenerator)
            : this(ffmpegFileName, processRunner, argumentGenerator, TimeSpan.FromSeconds(30))
        {

        }

        public InterlaceDetector(string ffmpegFileName,
                                 IProcessRunner processRunner,
                                 IFFmpegArgumentGenerator argumentGenerator,
                                 TimeSpan timeout)
        {
            _ffmpegFileName = ffmpegFileName;
            _processRunner = processRunner;
            _argumentGenerator = argumentGenerator;
            _timeout = timeout;
        }

        public async Task<bool> Detect(MediaInfo mediaInfo)
        {
            if (mediaInfo == null)
            {
                throw new ArgumentNullException(nameof(mediaInfo));
            }
            if (string.IsNullOrWhiteSpace(mediaInfo.FileName))
            {
                throw new ArgumentException($"{nameof(mediaInfo)}.FileName must not be empty or whitespace.", nameof(mediaInfo));
            }
            if (mediaInfo.Duration <= TimeSpan.Zero)
			{
				throw new ArgumentException($"{nameof(mediaInfo)}.Duration is invalid.", nameof(mediaInfo));
			}

            var job = new FFmpegJob()
            {
                HideBanner = true,
                StartTime = TimeSpan.FromMilliseconds(mediaInfo.Duration.TotalMilliseconds / 2),
                InputFileName = mediaInfo.FileName,
                FrameCount = FRAME_COUNT,
                Filters = new IFilter[]
                {
                    new Filter("idet")
                }
            };
            var arguments = _argumentGenerator.GenerateArguments(job);
            FrameStatistics statistics = null;

            try
            {
                var processResult = await _processRunner.Run(_ffmpegFileName, arguments, _timeout);

                //The interlace detection data is written to standard error.
                if (!string.IsNullOrWhiteSpace(processResult.ErrorData))
                {
                    statistics = Parse(processResult.ErrorData);
                }
                else
                {
                    Trace.WriteLine("No ffmpeg data on stderr to parse.");
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

            return statistics != null &&
                (statistics.TffCount + statistics.BffCount) > (statistics.ProgressiveCount + statistics.UndeterminedCount);
        }

        FrameStatistics Parse(string outputData)
        {
            FrameStatistics result = null;
            int tff, bff, progressive, undetermined;
            var match = Regex.Match(outputData,
                                    @"Multi frame detection:\s+" +
                                    $"TFF:\\s*(?<{nameof(tff)}>\\d+)\\s+" +
                                    $"BFF:\\s*(?<{nameof(bff)}>\\d+)\\s+" +
                                    $"Progressive:\\s*(?<{nameof(progressive)}>\\d+)\\s+" +
                                    $"Undetermined:\\s*(?<{nameof(undetermined)}>\\d+)",
                                    RegexOptions.IgnoreCase | RegexOptions.RightToLeft); //get the last match

            if (match.Success &&
                int.TryParse(match.Groups[nameof(tff)].Value, out tff) &&
                int.TryParse(match.Groups[nameof(bff)].Value, out bff) &&
                int.TryParse(match.Groups[nameof(progressive)].Value, out progressive) &&
                int.TryParse(match.Groups[nameof(undetermined)].Value, out undetermined))
            {
                result = new FrameStatistics()
                {
                    TffCount = tff,
                    BffCount = bff,
                    ProgressiveCount = progressive,
                    UndeterminedCount = undetermined
                };
            }
            else
            {
                Trace.WriteLine("No interlace data was found.");
            }

            return result;
        }
    }
}
