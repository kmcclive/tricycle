using System;
using Tricycle.Diagnostics.Utilities;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class FileNameConverter : ArgumentConverter
    {
        public override string Convert(string argName, object value)
        {
            if (value is string fileName)
            {
                string escapedPath = ProcessUtility.Self.EscapeFilePath(fileName);

                return base.Convert(argName, escapedPath);
            }

            throw new NotSupportedException($"{nameof(value)} must be of type string.");
        }
    }
}
