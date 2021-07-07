using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;

namespace Tricycle.Bridge.Console
{
    class Program
    {
        static AppServiceConnection connection;
        static AppServiceClosedStatus? closedStatus;

        static async Task<int> Main()
        {
            var status = await InitializeAppServiceConnection();

            if (status != AppServiceConnectionStatus.Success)
            {
                return 1;
            }

            do
            {
                await Task.Yield();
            } while (closedStatus == null);

            return closedStatus == AppServiceClosedStatus.Completed ? 0 : 1;
        }

        static async Task<AppServiceConnectionStatus> InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "TricycleProcessService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += OnRequestReceived;
            connection.ServiceClosed += OnServiceClosed;

            return await connection.OpenAsync();
        }

        static void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            closedStatus = args.Status;
        }

        static void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
