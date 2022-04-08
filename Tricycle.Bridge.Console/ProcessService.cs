using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tricycle.Bridge.Models;
using Tricycle.Diagnostics;
using Tricycle.IO;
using Tricycle.Utilities;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Tricycle.Bridge.Console
{
    public class ProcessService
    {
        readonly IAppServiceConnection _connection;
        readonly ISerializer<string> _serializer;
        readonly Func<IProcess> _processCreator;
        readonly IDictionary<int, IProcess> _processesById = new ConcurrentDictionary<int, IProcess>();
        readonly ManualResetEvent _closed = new ManualResetEvent(false);
        AppServiceClosedStatus? _closedStatus;

        public ProcessService(IAppServiceConnection connection, ISerializer<string> serializer, Func<IProcess> processCreator)
        {
            _connection = connection;
            _serializer = serializer;
            _processCreator = processCreator;

            _connection.RequestReceived += OnRequestReceived;
            _connection.ServiceClosed += OnServiceClosed;
        }

        public AppServiceClosedStatus Start()
        {
            _closedStatus = null;

            var task = _connection.OpenAsync().AsTask();

            task.Wait();

            if (task.Result != AppServiceConnectionStatus.Success)
            {
                return AppServiceClosedStatus.Canceled;
            }

            _closed.WaitOne();

            return _closedStatus.Value;
        }

        void OnServiceClosed(IAppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _closedStatus = args.Status;
            _closed.Set();
        }

        async void OnRequestReceived(IAppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var message = args.Request.Message;

            if (message == null)
            {
                await RespondWithInvalidReqest(args.Request);

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
                await RespondWithInvalidReqest(args.Request);

                return;
            }

            if (messageType == MessageType.StartProcess)
            {
                await args.Request.SendResponseAsync(HandleStartProcessRequest(body));
            }
            else if (messageType == MessageType.KillProcess)
            {
                await args.Request.SendResponseAsync(HandleKillProcessRequest(body));
            }
        }

        async Task RespondWithInvalidReqest(AppServiceRequest request)
        {
            var response = new ValueSet();

            response[MessageKey.Body] = SerializeBody(new Response(new Error(ErrorType.InvalidRequest)));

            await request.SendResponseAsync(response);
        }

        string SerializeBody(object body)
        {
            string result = null;

            try
            {
                result = _serializer.Serialize(body);
            }
            catch (SerializationException) { }

            return result;
        }

        Error MapToError(Exception ex)
        {
            return new Error(ex.GetType().ToString(), ex.Message);
        }

        (T, Error) DeserializeRequest<T>(string body)
        {
            T request = default;

            try
            {
                request = _serializer.Deserialize<T>(body);
            }
            catch (SerializationException ex)
            {
                return (request, MapToError(ex));
            }

            return (request, null);
        }

        ValueSet HandleStartProcessRequest(string body)
        {
            var (request, error) = DeserializeRequest<StartProcessRequest>(body);
            var response = new StartProcessResponse()
            {
                Error = error
            };
            var result = new ValueSet();

            if (request == null)
            {
                result[MessageKey.Body] = SerializeBody(response);

                return result;
            }

            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                response.Error = new Error(ErrorType.InvalidRequest, $"{nameof(request.FileName)} was not set");

                result[MessageKey.Body] = SerializeBody(response);
                
                return result;
            }

            var process = _processCreator.Invoke();
            var startInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = request.FileName,
                Arguments = request.Arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            process.OutputDataReceived += data => OnDataReceived<OutputDataMessage>(process, MessageType.OutputData, data);
            process.ErrorDataReceived += data => OnDataReceived<ErrorDataMessage>(process, MessageType.ErrorData, data);
            process.Exited += () => OnExited(process);

            try
            {
                process.Start(startInfo);

                _processesById[process.Id] = process;
                response.ProcessId = process.Id;
            }
            catch (ArgumentException ex)
            {
                response.Error = MapToError(ex);
            }
            catch (InvalidOperationException ex)
            {
                response.Error = MapToError(ex);
            }

            result[MessageKey.Body] = SerializeBody(response);

            return result;
        }

        ValueSet HandleKillProcessRequest(string body)
        {
            var (request, error) = DeserializeRequest<KillProcessRequest>(body);
            var response = new KillProcessResponse()
            {
                Error = error
            };
            var result = new ValueSet();

            if (request == null)
            {
                result[MessageKey.Body] = SerializeBody(response);

                return result;
            }

            var process = _processesById.GetValueOrDefault(request.ProcessId);

            if (process == null)
            {
                response.Error = new Error(ErrorType.InvalidRequest, $"process with ID {request.ProcessId} was not found");

                result[MessageKey.Body] = SerializeBody(response);

                return result;
            }

            try
            {
                lock (process)
                {
                    process.Kill();
                }
            }
            catch (InvalidOperationException ex)
            {
                response.Error = MapToError(ex);
            }

            result[MessageKey.Body] = SerializeBody(response);

            return result;
        }

        void OnDataReceived<T>(IProcess process, MessageType messageType, string data) where T : DataMessage<string>, new()
        {
            var body = new T
            {
                ProcessId = process.Id,
                Data = data
            };
            var message = new ValueSet();

            message[MessageKey.MessageType] = (int)messageType;
            message[MessageKey.Body] = SerializeBody(body);

            lock (process)
            {
                _connection.SendMessageAsync(message).AsTask().Wait();
            }
        }

        void OnExited(IProcess process)
        {
            var body = new ExitedMessage(process.Id, process.ExitCode);
            var message = new ValueSet();

            message[MessageKey.MessageType] = (int)MessageType.Exited;
            message[MessageKey.Body] = SerializeBody(body);

            lock (process)
            {
                _connection.SendMessageAsync(message).AsTask().Wait();
            }

            _processesById.Remove(process.Id);
            process.Dispose();
        }
    }
}
