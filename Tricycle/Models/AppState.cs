using StructureMap;

namespace Tricycle.Models
{
    public static class AppState
    {
        public static Container IocContainer { get; set; }
        public static TricycleConfig TricycleConfig { get; set; }
    }
}
