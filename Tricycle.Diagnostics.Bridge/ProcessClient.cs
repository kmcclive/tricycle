using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Tricycle.Bridge;
using Tricycle.Bridge.Models;
using Tricycle.IO;
using Tricycle.Utilities;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Tricycle.Diagnostics.Bridge
{
    public class ProcessClient : IProcess
    {
        readonly Func<IAppServiceConnection> _connectionProvider;
        readonly ISerializer<string> _serializer;
        readonly ManualResetEvent _exitedEvent = new ManualResetEvent(false);
        IAppServiceConnection _connection;

        public int Id { get; private set; }

        public int ExitCode { get; private set; }

        public bool HasExited { get; private set; }

        public event Action Exited;
        public event Action<string> ErrorDataReceived;
        public event Action<string> OutputDataReceived;

        public ProcessClient(Func<IAppServiceConnection> connectionProvider, ISerializer<string> serializer)
        {
            _connectionProvider = connectionProvider;
            _serializer = serializer;
        }

        public void Dispose()
        {
            Reset();
        }

        public void Kill()
        {
            if (Id == default)
            {
                throw new InvalidOperationException("The process was never started.");
            }

            var request = new KillProcessRequest()
            {
                ProcessId = Id
            };
            var response = SendRequest<KillProcessResponse>(MessageType.KillProcess, request);

            if (response.Error != null)
            {
                var errorString = FormatError(response.Error);

                throw new InvalidOperationException($"An error occurred killing the process: {errorString}");
            }
        }

        public bool Start(ProcessStartInfo startInfo)
        {
            if (Id != default)
            {
                if (HasExited)
                {
                    Reset();
                }
                else
                {
                    throw new InvalidOperationException("Process is already running.");
                }
            }

            _connection = InitializeConnection();

            Trace.WriteLine($"Starting process: {startInfo.FileName} {startInfo.Arguments}");

            var request = MapToRequest(startInfo);
            var response = SendRequest<StartProcessResponse>(MessageType.StartProcess, request);

            if (response.Error != null)
            {
                var errorString = FormatError(response.Error);

                throw new InvalidOperationException($"An error occurred starting the process: {errorString}");
            }

            Id = response.ProcessId;

            return true;
        }

        public void WaitForExit()
        {
            WaitForExit(-1);
        }

        public bool WaitForExit(int milliseconds)
        {
            if (!HasExited)
            {
                if (milliseconds > 0)
                {
                    _exitedEvent.WaitOne(milliseconds);
                }
                else
                {
                    _exitedEvent.WaitOne();
                }
            }

            return HasExited;
        }

        void OnRequestReceived(IAppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var message = args.Request.Message;

            if (message == null)
            {
                return;
            }

            MessageType? messageType = null;

            if (message.TryGetValue(MessageKey.MessageType, out var temp) && temp is int)
            {
                messageType = (MessageType)temp;
            }

            var body = message.GetValueOrDefault(MessageKey.Body) as string;

            if (!messageType.HasValue || string.IsNullOrWhiteSpace(body))
            {
                return;
            }

            switch (messageType.Value)
            {
                case MessageType.Exited:
                    HandleExitedMessage(body);
                    break;
                case MessageType.ErrorData:
                    HandleErrorDataMessage(body);
                    break;
                case MessageType.OutputData:
                    HandleOutputDataMessage(body);
                    break;
            }
        }

        IAppServiceConnection InitializeConnection()
        {
            var connection = _connectionProvider.Invoke();

            if (connection == null)
            {
                throw new InvalidOperationException("No connection to bridge process was found.");
            }

            connection.RequestReceived += OnRequestReceived;

            return connection;
        }

        StartProcessRequest MapToRequest(ProcessStartInfo startInfo)
        {
            return new StartProcessRequest
            {
                FileName = startInfo.FileName,
                Arguments = startInfo.Arguments
            };
        }

        void HandleExitedMessage(string body)
        {
            var message = DeserializeBody<ExitedMessage>(body);

            if (message != null && IsSameProcess(message.ProcessId))
            {
                ExitCode = message.ExitCode;
                HasExited = true;
                _exitedEvent.Set();
                Exited?.Invoke();
            }
        }

        void HandleErrorDataMessage(string body)
        {
            var message = DeserializeBody<ErrorDataMessage>(body);

            if (message != null && IsSameProcess(message.ProcessId))
            {
                Trace.WriteLine($"[stderr] {message.Data}");
                ErrorDataReceived?.Invoke(message.Data);
            }
        }

        void HandleOutputDataMessage(string body)
        {
            var message = DeserializeBody<OutputDataMessage>(body);

            if (message != null && IsSameProcess(message.ProcessId))
            {
                Trace.WriteLine($"[stdout] {message.Data}");
                OutputDataReceived?.Invoke(message.Data);
            }
        }

        T DeserializeBody<T>(string body)
        {
            T result = default;

            try
            {
                result = _serializer.Deserialize<T>(body);
            }
            catch (SerializationException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return result;
        }

        bool IsSameProcess(int processId)
        {
            return Id != default && processId == Id;
        }

        T SendRequest<T>(MessageType messageType, object request)
        {
            string body;

            try
            {
                body = _serializer.Serialize(request);
            }
            catch (SerializationException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                throw new InvalidOperationException($"An error occurred serializing the {messageType} request.", ex);
            }

            var message = new ValueSet()
            {
                { MessageKey.MessageType, (int)messageType },
                { MessageKey.Body, body }
            };
            var result = _connection.SendMessageAsync(message).AsTask().RunSync();

            if (result.Status != AppServiceResponseStatus.Success)
            {
                throw new InvalidOperationException($"An error occurred sending the {messageType} request.");
            }

            var responseBody = result.Message?.GetValueOrDefault(MessageKey.Body) as string;

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                throw new InvalidOperationException($"No response was received for the {messageType} request.");
            }

            try
            {
                return _serializer.Deserialize<T>(responseBody);
            }
            catch (SerializationException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                throw new InvalidOperationException($"An error occurred deserializing the {messageType} response.", ex);
            }
        }

        string FormatError(Error error)
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(error.ErrorType))
            {
                builder.Append($"({error.ErrorType})");
            }

            if (!string.IsNullOrWhiteSpace(error.Message))
            {
                if (builder.Length > 0)
                {
                    builder.Append(" ");
                }

                builder.Append(error.Message);
            }

            if (builder.Length < 1)
            {
                builder.Append("Unknown");
            }

            return builder.ToString();
        }

        void Reset()
        {
            Id = default;
            ExitCode = default;
            HasExited = false;
            _exitedEvent.Reset();
        }
    }
}
