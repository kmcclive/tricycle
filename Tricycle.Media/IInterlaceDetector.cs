using System;
using System.Threading.Tasks;
using Tricycle.Models.Media;

namespace Tricycle.Media
{
    public interface IInterlaceDetector
    {
        /// <summary>
        /// Detects whether a video is interlaced.
        /// </summary>
        /// <param name="mediaInfo">The info of the media to scan.</param>
        /// <returns><c>true</c> if the video is interlaced; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="mediaInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="mediaInfo"/> is invalid.</exception>
        Task<bool> Detect(MediaInfo mediaInfo);
    }
}
