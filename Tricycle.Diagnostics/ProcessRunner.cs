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
        readonly TimeSpan _timeout;

        public ProcessRunner(Func<IProcess> processCreator, TimeSpan timeout)
        {
            _processCreator = processCreator;
            _timeout = timeout;
        }

        public ProcessResult Run(string fileName)
        {
            return Run(fileName, null);
        }

        public ProcessResult Run(string fileName, string arguments)
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
                var completion = new ManualResetEvent(false);

                process.OutputDataReceived += (data) => outputBuilder.AppendLine(data);
                process.ErrorDataReceived += (data) => errorBuilder.AppendLine(data);
                process.Exited += () => completion.Set();

                process.Start(startInfo);

                if (!completion.WaitOne(_timeout))
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
