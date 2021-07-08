using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Tricycle.Bridge
{
    public class AppServiceConnectionWrapper : IAppServiceConnection
    {
        AppServiceConnection _connection;

        public string AppServiceName
        {
            get => _connection.AppServiceName;
            set
            {
                _connection.AppServiceName = value;
            }
        }
        public string PackageFamilyName
        {
            get => _connection.PackageFamilyName;
            set
            {
                _connection.PackageFamilyName = value;
            }
        }

        public event TypedEventHandler<IAppServiceConnection, AppServiceRequestReceivedEventArgs> RequestReceived;
        public event TypedEventHandler<IAppServiceConnection, AppServiceClosedEventArgs> ServiceClosed;

        public AppServiceConnectionWrapper()
            : this(new AppServiceConnection())
        {

        }

        public AppServiceConnectionWrapper(AppServiceConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _connection.RequestReceived += OnRequestReceived;
            _connection.ServiceClosed += OnServiceClosed;
        }

        public IAsyncOperation<AppServiceConnectionStatus> OpenAsync()
        {
            return _connection.OpenAsync();
        }

        public IAsyncOperation<AppServiceResponse> SendMessageAsync(ValueSet message)
        {
            return _connection.SendMessageAsync(message);
        }

        void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            RequestReceived?.Invoke(this, args);
        }

        void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            ServiceClosed?.Invoke(this, args);
        }
    }
}
