using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
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

                return true;
            }
        }

        #endregion

        [TestMethod]
        public void TestRun()
        {
            var process = new MockProcess();
            Func<IProcess> processCreator = () => process;
            var processUtility = Substitute.For<IProcessUtility>();
            var timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS);
            var runner = new ProcessRunner(processCreator, timeout);

            Assert.ThrowsException<ArgumentNullException>(() => runner.Run(null));

            var result = runner.Run(EXE_FILE_NAME, ARGS_1);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.ExitCode);
            Assert.AreEqual(OUTPUT + Environment.NewLine, result.OutputData);
            Assert.AreEqual(ERRORS + Environment.NewLine, result.ErrorData);

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            result = runner.Run(EXE_FILE_NAME, ARGS_2);

            stopwatch.Stop();

            Assert.IsTrue(stopwatch.Elapsed > timeout, "The specified timeout was not honored.");
        }
    }
}
