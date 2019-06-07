using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Tricycle.Diagnostics.Utilities
{
    public class ProcessUtility : IProcessUtility
    {
        public static ProcessUtility Self { get; } = new ProcessUtility();

        private ProcessUtility()
        {

        }

        public string EscapeFilePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"\"{Regex.Replace(path, @"(\\+)$", @"$1$1")}\"";
            }

            return path.Replace(" ", @"\ ");
        }
    }
}
