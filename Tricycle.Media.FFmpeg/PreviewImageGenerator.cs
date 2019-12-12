using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Tricycle.Diagnostics;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Models.Jobs;

namespace Tricycle.Media.FFmpeg
{
    public class PreviewImageGenerator : FFmpegJobRunnerBase, IPreviewImageGenerator
    {
        readonly string _ffmpegFileName;
        readonly IProcessRunner _processRunner;
        readonly IFileSystem _fileSystem;
        readonly int _imageCount;
        readonly TimeSpan _timeout;

        public PreviewImageGenerator(string ffmpegFileName,
                                     IProcessRunner processRunner,
                                     IFFmpegArgumentGenerator argumentGenerator,
                                     IConfigManager<FFmpegConfig> configManager,
                                     IFileSystem fileSystem)
            : this(ffmpegFileName, processRunner, argumentGenerator, configManager, fileSystem, 5)
        {

        }

        public PreviewImageGenerator(string ffmpegFileName,
                                     IProcessRunner processRunner,
                                     IFFmpegArgumentGenerator argumentGenerator,
                                     IConfigManager<FFmpegConfig> configManager,
                                     IFileSystem fileSystem,
                                     int imageCount)
            : this(ffmpegFileName,
                   processRunner,
                   argumentGenerator,
                   configManager,
                   fileSystem,
                   imageCount,
                   TimeSpan.FromSeconds(30))
        {

        }

        public PreviewImageGenerator(string ffmpegFileName,
                                     IProcessRunner processRunner,
                                     IFFmpegArgumentGenerator argumentGenerator,
                                     IConfigManager<FFmpegConfig> configManager,
                                     IFileSystem fileSystem,
                                     int imageCount,
                                     TimeSpan timeout)
            : base(configManager, argumentGenerator)
        {
            _ffmpegFileName = ffmpegFileName;
            _processRunner = processRunner;
            _fileSystem = fileSystem;
            _imageCount = imageCount;
            _timeout = timeout;
        }

        public async Task<IList<string>> Generate(TranscodeJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (job.SourceInfo?.Duration <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    $"{nameof(job)}.{nameof(job.SourceInfo)}.{nameof(job.SourceInfo.Duration)} is invalid.",
                    nameof(job));
            }

            var dictionary = new ConcurrentDictionary<TimeSpan, string>();
            var positions = GetPositions(job.SourceInfo.Duration);
            string tempPath = _fileSystem.Path.GetTempPath();

            var tasks = positions.Select(async position =>
            {
                var ffmpegJob = Map(job, _configManager?.Config);

                ffmpegJob.LogLevel = "panic";
                ffmpegJob.OutputFileName = _fileSystem.Path.Combine(tempPath, $"{Guid.NewGuid()}.png");
                ffmpegJob.StartTime = position;
                ffmpegJob.FrameCount = 1;

                string arguments = _argumentGenerator.GenerateArguments(ffmpegJob);

                try
                {
                    var processResult = await _processRunner.Run(_ffmpegFileName, arguments, _timeout);

                    if ((processResult.ExitCode == 0) && _fileSystem.File.Exists(ffmpegJob.OutputFileName))
                    {
                        dictionary[position] = ffmpegJob.OutputFileName;
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
            });

            await Task.WhenAll(tasks);

            return dictionary.OrderBy(p => p.Key).Select(p => p.Value).ToArray();
        }

        protected virtual IEnumerable<TimeSpan> GetPositions(TimeSpan duration)
        {
            var interval = duration.TotalMilliseconds / (_imageCount + 1);

            return Enumerable.Range(1, _imageCount).Select(x => TimeSpan.FromMilliseconds(x * interval));
        }
    }
}
