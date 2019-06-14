using System;
using Tricycle.IO;
using Tricycle.Media;

namespace Tricycle.Models
{
    public static class AppState
    {
        public static IFileBrowser FileBrowser { get; set; }
        public static IMediaInspector MediaInspector { get; set; }
    }
}
