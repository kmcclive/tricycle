using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Tricycle.Diagnostics.Models;

namespace Tricycle.Diagnostics
{
    public class ProcessRunner : IProcessRunner
    {
        readonly Func<IProcess> _processCreator;

        public ProcessRunner(Func<IProcess> processCreator)
        {
            _processCreator = processCreator;
        }

        public ProcessResult Run(string fileName)
        {
            return Run(fileName, null);
        }

        public ProcessResult Run(string fileName, string arguments)
        {
            return Run(fileName, arguments, null);
        }

        public ProcessResult Run(string fileName, string arguments, TimeSpan? timeout)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            ProcessResult result = null;

            using (IProcess process = _processCreator.Invoke())
            {
                var startInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                bool incomplete = false;

                using (var completion = new ManualResetEvent(false))
                {
                    process.OutputDataReceived += (data) => outputBuilder.AppendLine(data);
                    process.ErrorDataReceived += (data) => errorBuilder.AppendLine(data);
                    process.Exited += () => {
                        try
                        {
                            completion?.Set();
                        }
                        catch (ObjectDisposedException) { }
                    };

                    process.Start(startInfo);

                    if (timeout.HasValue)
                    {
                        incomplete = !completion.WaitOne(timeout.Value);
                    }
                    else
                    {
                        completion.WaitOne();
                    }
                }

                if (incomplete)
                {
                    process.Kill();
                }

                result = new ProcessResult()
                {
                    OutputData = outputBuilder.ToString(),
                    ErrorData = errorBuilder.ToString(),
                    ExitCode = process.ExitCode
                };
            }

            return result;
        }
    }
}
