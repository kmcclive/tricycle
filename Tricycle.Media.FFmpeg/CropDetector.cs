using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Utilities;
using Tricycle.Models;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg
{
    public class CropDetector : ICropDetector
    {
        static readonly TimeSpan MAX_SEEK_TIME = TimeSpan.FromMinutes(5);

        readonly string _ffmpegFileName;
        readonly IProcessRunner _processRunner;
        readonly IProcessUtility _processUtility;
        readonly TimeSpan _timeout;

        public CropDetector(string ffmpegFileName,
                            IProcessRunner processRunner,
                            IProcessUtility processUtility)
            : this(ffmpegFileName, processRunner, processUtility, TimeSpan.FromSeconds(30))
        {

        }

        public CropDetector(string ffmpegFileName,
                            IProcessRunner processRunner,
                            IProcessUtility processUtility,
                            TimeSpan timeout)
        {
            _ffmpegFileName = ffmpegFileName;
            _processRunner = processRunner;
            _processUtility = processUtility;
            _timeout = timeout;
        }

        public async Task<CropParameters> Detect(MediaInfo mediaInfo)
        {
            if (mediaInfo == null)
            {
                throw new ArgumentNullException(nameof(mediaInfo));
            }
            if (string.IsNullOrWhiteSpace(mediaInfo.FileName))
            {
                throw new ArgumentException($"{nameof(mediaInfo)}.FileName must not be empty or whitespace.", nameof(mediaInfo));
            }
            if (mediaInfo.Duration.TotalSeconds < 2)
            {
                throw new ArgumentException($"{nameof(mediaInfo)}.Duration is invalid.", nameof(mediaInfo));
            }

            CropParameters result = null;
            int seconds = GetSeekSeconds(mediaInfo.Duration);

            var escapedFileName = _processUtility.EscapeFilePath(mediaInfo.FileName);
            var arguments = $"-hide_banner -ss {seconds} -i {escapedFileName} -frames:vf 2 -vf cropdetect -f null -";

            try
            {
                var processResult = await _processRunner.Run(_ffmpegFileName, arguments, _timeout);

                //The crop detection data is written to standard error.
                if (!string.IsNullOrWhiteSpace(processResult.ErrorData))
                {
                    result = Parse(processResult.ErrorData);

                    var originalDimensions = mediaInfo.Streams?.OfType<VideoStreamInfo>().FirstOrDefault()?.Dimensions;

                    if ((result != null) && (originalDimensions != null))
                    {
                        result = Correct(result, originalDimensions.Value);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex);
            }

            return result;
        }

        int GetSeekSeconds(TimeSpan duration)
        {
            int seconds = (int)Math.Round(duration.TotalSeconds / 2);

            if (TimeSpan.FromSeconds(seconds) > MAX_SEEK_TIME)
            {
                seconds = (int)Math.Round(MAX_SEEK_TIME.TotalSeconds);
            }

            return seconds;
        }

        CropParameters Parse(string outputData)
        {
            CropParameters result = null;
            int x, y, width, height;
            var match = Regex.Match(outputData,
                $"crop=(?<{nameof(width)}>\\d+):(?<{nameof(height)}>\\d+):(?<{nameof(x)}>\\d+):(?<{nameof(y)}>\\d+)");

            if (match.Success &&
                int.TryParse(match.Groups[nameof(x)].Value, out x) &&
                int.TryParse(match.Groups[nameof(y)].Value, out y) &&
                int.TryParse(match.Groups[nameof(width)].Value, out width) &&
                int.TryParse(match.Groups[nameof(height)].Value, out height))
            {
                result = new CropParameters()
                {
                    Start = new Coordinate<int>(x, y),
                    Size = new Dimensions(width, height)
                };
            }

            return result;
        }

        // FFmpeg will crop to adjust the size to a number divisible by 16.
        // That feature is supposed to be configurable, but modifying it seems to break the detection.
        // This code is meant to correct that.
        CropParameters Correct(CropParameters cropParameters, Dimensions originalDimensions)
        {
            const int THRESHOLD = 16;

            int x = cropParameters.Start.X;
            int y = cropParameters.Start.Y;
            int width = cropParameters.Size.Width;
            int height = cropParameters.Size.Height;
            int widthDelta = originalDimensions.Width - cropParameters.Size.Width;
            int heightDelta = originalDimensions.Height - cropParameters.Size.Height;

            if ((widthDelta > 0) && (widthDelta < THRESHOLD))
            {
                x = 0;
                width = originalDimensions.Width;
            }

            if ((heightDelta > 0) && (heightDelta < THRESHOLD))
            {
                y = 0;
                height = originalDimensions.Height;
            }

            return new CropParameters()
            {
                Start = new Coordinate<int>(x, y),
                Size = new Dimensions(width, height)
            };
        }
    }
}
