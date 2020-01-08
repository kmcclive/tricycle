using System;
using System.Collections.Generic;
using System.Text;
using Tricycle.Media.FFmpeg.Models.Jobs;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class OptionListConverter : ArgumentConverter
    {
        public override string Convert(string argName, object value)
        {
            if (value is IEnumerable<Option> options)
            {
                var builder = new StringBuilder();

                foreach (var option in options)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(":");
                    }

                    bool hasName = false;

                    if (!string.IsNullOrWhiteSpace(option?.Name))
                    {
                        hasName = true;
                        builder.Append($"{option.Name}");
                    }

                    if (!string.IsNullOrWhiteSpace(option?.Value))
                    {
                        if (hasName)
                        {
                            builder.Append("=");
                        }
                        builder.Append(EscapeValue(option.Value));
                    }
                }

                return builder.Length > 0 ? base.Convert(argName, builder) : string.Empty;
            }

            throw new NotSupportedException($"{nameof(value)} must be of type IEnumerable<Option>.");
        }

        string EscapeValue(string value)
        {
            string result = value;
            bool quoteDelimited = false;

            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                result = result.Substring(1, value.Length - 2);
                quoteDelimited = true;
            }

            // backslash needs to be first
            result = result.Replace(@"\", @"\\\\");
            result = result.Replace("\"", "\\\\\"");
            result = result.Replace("'", @"\\\'");
            result = result.Replace(":", @"\\:");

            if (quoteDelimited)
            {
                result = $"\"{result}\"";
            }

            return result;
        }
    }
}
