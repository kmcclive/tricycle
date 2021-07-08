using System;
using System.Threading.Tasks;
using Tricycle.Diagnostics;
using Tricycle.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;

namespace Tricycle.Bridge.Console
{
    class Program
    {
        static int Main()
        {
            var connection = InitializeConnection();
            var serializer = new JsonSerializer(new Newtonsoft.Json.JsonSerializerSettings());
            var service = new ProcessService(connection, serializer, () => new ProcessWrapper());

            return service.Start() == AppServiceClosedStatus.Completed ? 0 : 1;
        }

        static IAppServiceConnection InitializeConnection()
        {
            return new AppServiceConnectionWrapper()
            {
                AppServiceName = "TricycleProcessService",
                PackageFamilyName = Package.Current.Id.FamilyName
            };
        }
    }
}
