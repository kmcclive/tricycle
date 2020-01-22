using System;
using System.Runtime.InteropServices;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public static class ArgumentSerializationUtility
    {
        public static string EscapeValue(string value)
        {
            string result = value;
            bool quoteDelimited = false;

            if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal))
            {
                result = result.Substring(1, value.Length - 2);
                quoteDelimited = true;
            }

            // backslash needs to be first
            result = result.Replace(@"\", @"\\\\");
            result = result.Replace("\"", "\\\\\\\"");
            result = result.Replace("'", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\\\'" : @"\\\\\'");
            result = result.Replace(":", @"\\:");

            if (quoteDelimited)
            {
                result = $"\"{result}\"";
            }

            return result;
        }
    }
}
