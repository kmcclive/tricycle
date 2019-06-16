using System;
using Xamarin.Forms;
using Tricycle.IO;
using Tricycle.Models;
using StructureMap;
using Tricycle.Media;
using Tricycle.Media.FFmpeg;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Utilities;

namespace Tricycle.Bootstrap.macOS
{
    // This class (and project) is a workaround for a build error received
    // when calling similar code from a Xamarin.Mac project.
    public static class Bootstrapper
    {
        public static void Run(IFileBrowser fileBrowser, string ffprobeFileName)
        {
            var processCreator = new Func<IProcess>(() => new ProcessWrapper());

            AppState.IocContainer = new Container(_ =>
            {
                _.For<IFileBrowser>().Use(fileBrowser);
                _.For<IMediaInspector>().Use(new MediaInspector(ffprobeFileName, processCreator, ProcessUtility.Self));
            });
            AppState.TricycleConfig = new TricycleConfig();
        }
    }
}
