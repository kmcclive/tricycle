using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Tricycle.Bridge
{
    public interface IAppServiceConnection
    {
        string AppServiceName { get; set; }
        string PackageFamilyName { get; set; }

        IAsyncOperation<AppServiceConnectionStatus> OpenAsync();
        IAsyncOperation<AppServiceResponse> SendMessageAsync(ValueSet message);

        event TypedEventHandler<IAppServiceConnection, AppServiceRequestReceivedEventArgs> RequestReceived;
        event TypedEventHandler<IAppServiceConnection, AppServiceClosedEventArgs> ServiceClosed;
    }
}
