using System;
using System.Threading.Tasks;
using Tricycle.Models;
using Tricycle.Models.Media;

namespace Tricycle.Media
{
    public interface ICropDetector
    {
        /// <summary>
        /// Detects crop parameters required to remove black bars.
        /// </summary>
        /// <param name="mediaInfo">The info of the media to scan.</param>
        /// <returns>The crop parameters required to remove black bars.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="mediaInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="mediaInfo"/> is invalid.</exception>
        Task<CropParameters> Detect(MediaInfo mediaInfo);
    }
}
