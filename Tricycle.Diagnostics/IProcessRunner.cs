﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tricycle.Diagnostics.Models;

namespace Tricycle.Diagnostics
{
    public interface IProcessRunner
    {
        /// <summary>
        /// Runs a process and waits for exit.
        /// </summary>
        /// <returns>The result of running the process.</returns>
        /// <param name="fileName">The name of the file to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="fileName"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">An error occurred starting the process.</exception>
        Task<ProcessResult> Run(string fileName);

        /// <summary>
        /// Runs a process and waits for exit.
        /// </summary>
        /// <returns>The result of running the process.</returns>
        /// <param name="fileName">The name of the file to execute.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="fileName"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">An error occurred starting the process.</exception>
        Task<ProcessResult> Run(string fileName, string arguments);

        /// <summary>
        /// Runs a process and waits for exit until a specified timeout.
        /// </summary>
        /// <returns>The result of running the process.</returns>
        /// <param name="fileName">The name of the file to execute.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <param name="timeout">The timeout to use for the process.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="fileName"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">An error occurred starting the process.</exception>
        Task<ProcessResult> Run(string fileName, string arguments, TimeSpan? timeout);
    }
}
