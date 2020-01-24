using System;
using StructureMap;

namespace Tricycle.Models
{
    public static class AppState
    {
        public static string AppName { get; set; }
        public static Version AppVersion { get; set; }
        public static Container IocContainer { get; set; }
        public static string DefaultDestinationDirectory { get; set; }
    }
}
