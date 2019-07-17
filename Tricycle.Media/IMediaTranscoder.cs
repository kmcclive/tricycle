using System;
using Tricycle.Models.Jobs;

namespace Tricycle.Media
{
    public interface IMediaTranscoder
    {
        /// <summary>
        /// Gets a value indicating whether a job is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Occurs when the status changes.
        /// </summary>
        event Action<TranscodeStatus> StatusChanged;

        /// <summary>
        /// Occurs when the job completes.
        /// </summary>
        event Action Completed;

        /// <summary>
        /// Occurs when the job fails.
        /// </summary>
        event Action<string> Failed;

        /// <summary>
        /// Starts a specified transcode job.
        /// </summary>
        /// <param name="job">The job to start.</param>
        /// <exception cref="ArgumentNullException"><paramref name="job"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="job"/> is incomplete or invalid.</exception>
        /// <exception cref="NotSupportedException"><paramref name="job"/> contains unsupported options.</exception>
        /// <exception cref="InvalidOperationException">
        /// A job is already running
        /// OR
        /// an error occurred starting the job.
        /// </exception>
        void Start(TranscodeJob job);

        /// <summary>
        /// Stops the currently-running job.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// No job is running
        /// OR
        /// an error occurred stopping the job.
        /// </exception>
        void Stop();
    }
}
