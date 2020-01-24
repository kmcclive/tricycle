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
                        builder.Append(ArgumentSerializationUtility.EscapeValue(option.Value));
                    }
                }

                return builder.Length > 0 ? base.Convert(argName, builder) : string.Empty;
            }

            throw new NotSupportedException($"{nameof(value)} must be of type IEnumerable<Option>.");
        }
    }
}
