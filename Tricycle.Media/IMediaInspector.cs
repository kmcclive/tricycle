using System;
using Tricycle.Media.Models;

namespace Tricycle.Media
{
    public interface IMediaInspector
    {
        /// <summary>
        /// Inspects a file with a specified name.
        /// </summary>
        /// <returns>Information about the file if found; otherwise, <c>null</c>.</returns>
        /// <param name="fileName">The name of the file to inspect.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c>.</exception>
        MediaInfo Inspect(string fileName);
    }
}
