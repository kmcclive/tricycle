using StructureMap;

namespace Tricycle.Models
{
    public static class AppState
    {
        public static Container IocContainer { get; set; }
        public static string DefaultDestinationDirectory { get; set; }
    }
}
