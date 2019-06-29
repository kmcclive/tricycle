using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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

        public Task<ProcessResult> Run(string fileName)
        {
            return Run(fileName, null);
        }

        public Task<ProcessResult> Run(string fileName, string arguments)
        {
            return Run(fileName, arguments, null);
        }

        public Task<ProcessResult> Run(string fileName, string arguments, TimeSpan? timeout)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            return Task.Run(() =>
            {
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

                    process.OutputDataReceived += (data) =>
                    {
                        Debug.WriteLine(data);
                        outputBuilder.AppendLine(data);
                    };
                    process.ErrorDataReceived += (data) =>
                    {
                        Debug.WriteLine(data);
                        errorBuilder.AppendLine(data);
                    };

                    process.Start(startInfo);

                    bool complete = false;

                    if (timeout.HasValue)
                    {
                        if (process.WaitForExit((int)timeout.Value.TotalMilliseconds))
                        {
                            // This is a workaround for a bug in the .NET code.
                            // See https://stackoverflow.com/a/25772586/9090758 for more details.
                            process.WaitForExit();
                            complete = true;
                        }
                    }
                    else
                    {
                        process.WaitForExit();
                    }

                    if (!complete)
                    {
                        Debug.WriteLine($"Process ({fileName}) timed out. Killing...");
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
            });
        }
    }
}
