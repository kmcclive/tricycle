using System;
namespace Tricycle.Diagnostics.Utilities
{
    public interface IProcessUtility
    {
        /// <summary>
        /// Escapes a specified file path for use in process arguments.
        /// </summary>
        /// <returns>The escaped file path.</returns>
        /// <param name="path">The file path to escape.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null.</c></exception>
        string EscapeFilePath(string path);
    }
}
