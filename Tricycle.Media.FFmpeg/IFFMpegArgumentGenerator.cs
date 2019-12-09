using System;
using Tricycle.Media.FFmpeg.Models.Jobs;

namespace Tricycle.Media.FFmpeg
{
    public interface IFFmpegArgumentGenerator
    {
        /// <summary>
        /// Generates arguments for a specified FFmpeg job.
        /// </summary>
        /// <param name="job">The job to generate arguments for.</param>
        /// <returns>The generated arguments.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="job"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="job"/>.InputFileName is <c>null</c> or empty
        /// </exception>
        string GenerateArguments(FFmpegJob job);
    }
}
