using System;
using Tricycle.Models.Jobs;

namespace Tricycle.Media.FFmpeg
{
    public interface IFFmpegArgumentGenerator
    {
        /// <summary>
        /// Generates arguments for a specified transcode job.
        /// </summary>
        /// <param name="job">The job to generate arguments for.</param>
        /// <returns>The generated arguments.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="job"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="job"/>.SourceInfo is <c>null</c> or missing data
        /// OR
        /// <paramref name="job"/>.OutputFileName is null or empty
        /// OR
        /// <paramref name="job"/>.Streams is null or empty
        /// </exception>
        /// <exception cref="NotSupportedException">An unsupported option was specified.</exception>
        string GenerateArguments(TranscodeJob job);
    }
}
