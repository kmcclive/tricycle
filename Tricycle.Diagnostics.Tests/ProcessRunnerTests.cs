using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics.Utilities;

namespace Tricycle.Diagnostics.Tests
{
    [TestClass]
    public class ProcessRunnerTests
    {
        #region Constants

        const string EXE_FILE_NAME = "exe";
        const string ARGS_1 = "args1";
        const string ARGS_2 = "args2";
        const string OUTPUT =
            @"line 1
              line 2
              line 3
              line 4
              line 5";
        const string ERRORS =
            @"error 1
              error 2
              error 3";
        const int TIMEOUT_MS = 100;

        #endregion

        #region Nested Types

        private class MockProcess : IProcess
        {
            ManualResetEvent _completion = new ManualResetEvent(false);

            public int Id { get; set; }
            public int ExitCode { get; set; }
            public bool HasExited { get; set; }

            public event Action Exited;
            public event Action<string> ErrorDataReceived;
            public event Action<string> OutputDataReceived;

            public void Dispose()
            {

            }

            public void Kill()
            {
                ExitCode = 1;
                HasExited = true;
            }

            public bool Start(ProcessStartInfo startInfo)
            {
                if (startInfo == null)
                {
                    throw new ArgumentNullException();
                }

                if (startInfo.FileName != EXE_FILE_NAME)
                {
                    throw new InvalidOperationException();
                }

                Task.Run(() =>
                {
                    var output = string.Empty;
                    var errors = string.Empty;

                    switch (startInfo.Arguments)
                    {
                        case ARGS_1:
                            output = OUTPUT;
                            errors = ERRORS;
                            break;
                        case ARGS_2:
                            Thread.Sleep(TIMEOUT_MS + 5);
                            break;
                    }

                    if (!startInfo.UseShellExecute)
                    {
                        using (var outputReader = new StringReader(output))
                        {
                            using (var errorReader = new StringReader(errors))
                            {
                                string outputLine = null;
                                string errorLine = null;

                                do
                                {
                                    Thread.Sleep(5);

                                    outputLine = outputReader.ReadLine();
                                    errorLine = errorReader.ReadLine();

                                    if (startInfo.RedirectStandardOutput && !string.IsNullOrEmpty(outputLine))
                                    {
                                        OutputDataReceived?.Invoke(outputLine);
                                    }

                                    if (startInfo.RedirectStandardError && !string.IsNullOrEmpty(errorLine))
                                    {
                                        ErrorDataReceived?.Invoke(errorLine);
                                    }
                                } while ((outputLine != null) || (errorLine != null));
                            }
                        }
                    }

                    ExitCode = 0;
                    HasExited = true;
                    Exited?.Invoke();
                    _completion.Set();
                });

                return true;
            }

            public void WaitForExit()
            {
                WaitForExit(-1);
            }

            public bool WaitForExit(int milliseconds)
            {
                if (milliseconds > 0)
                {
                    return _completion.WaitOne(milliseconds);
                }

                _completion.WaitOne();
                return true;
            }
        }

        #endregion

        [TestMethod]
        public async Task TestRun()
        {
            Func<IProcess> processCreator = () => new MockProcess();
            var processUtility = Substitute.For<IProcessUtility>();
            var timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS);
            var runner = new ProcessRunner(processCreator);

            #region Test exceptions

            Assert.ThrowsException<ArgumentNullException>(() => runner.Run(null));

            #endregion

            #region Test successful process with output and error data

            var result = await runner.Run(EXE_FILE_NAME, ARGS_1, TimeSpan.FromSeconds(1));

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.ExitCode);
            Assert.AreEqual(OUTPUT + Environment.NewLine, result.OutputData);
            Assert.AreEqual(ERRORS + Environment.NewLine, result.ErrorData);

            #endregion

            #region Test process that times out

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            result = await runner.Run(EXE_FILE_NAME, ARGS_2, timeout);

            stopwatch.Stop();

            Assert.IsTrue(stopwatch.Elapsed > timeout, "The specified timeout was not honored.");
            Assert.AreEqual(1, result.ExitCode);
            Assert.AreEqual(string.Empty, result.OutputData);
            Assert.AreEqual(string.Empty, result.ErrorData);

            #endregion
        }
    }
}
