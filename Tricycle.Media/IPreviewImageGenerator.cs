using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tricycle.Models.Jobs;

namespace Tricycle.Media
{
    public interface IPreviewImageGenerator
    {
        /// <summary>
        /// Generates preview images for a specified job.
        /// </summary>
        /// <param name="job">The job to generate preview images for.</param>
        /// <returns>An <see cref="IList{T}"/> containing the file names of the images generated.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="job"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="job"/> is incomplete or invalid.</exception>
        /// <exception cref="NotSupportedException"><paramref name="job"/> contains unsupported options.</exception>
        Task<IList<string>> Generate(TranscodeJob job);
    }
}
