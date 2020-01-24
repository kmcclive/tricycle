using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class MetadataConverter : ArgumentConverter
    {
        public override string Convert(string argName, object value)
        {
            if (value is IDictionary<string, string> dictionary)
            {
                var builder = new StringBuilder();

                foreach (var pair in dictionary)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(" ");
                    }

                    string escapedValue = ArgumentSerializationUtility.EscapeValue($"\"{pair.Value}\"");

                    builder.Append(base.Convert(argName, $"{pair.Key}={escapedValue}"));
                }

                return builder.ToString() ?? string.Empty;
            }

            throw new NotSupportedException($"{nameof(value)} must be of type IDictionary<string, string>.");
        }
    }
}
