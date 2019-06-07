using System;
using System.Diagnostics;

namespace Tricycle.Diagnostics
{
    /// <summary>
    /// Provides access to system processes.
    /// </summary>
    public interface IProcess : IDisposable
    {
        bool HasExited { get; }

        /// <summary>
        /// Starts the process.
        /// </summary>
        /// <returns>The start.</returns>
        /// <param name="startInfo">The info used to start the process.</param>
        /// <exception cref="ArgumentNullException"><paramref name="startInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="startInfo"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">An error occurred starting the process.</exception>
        bool Start(ProcessStartInfo startInfo);

        /// <summary>
        /// Kill the process.
        /// </summary>
        /// <exception cref="InvalidOperationException">An error occurred killing the process.</exception>
        void Kill();

        /// <summary>
        /// Occurs when the process exits.
        /// </summary>
        event Action Exited;

        /// <summary>
        /// Occurs when error data is received.
        /// </summary>
        event Action<string> ErrorDataReceived;

        /// <summary>
        /// Occurs when output data is received.
        /// </summary>
        event Action<string> OutputDataReceived;
    }
}
