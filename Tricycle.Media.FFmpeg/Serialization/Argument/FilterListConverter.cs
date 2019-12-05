using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Tricycle.Media.FFmpeg.Models.Jobs;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class FilterListConverter : ArgumentConverter, IArgumentConverter
    {
        public override string Convert(string argName, object value)
        {
            var builder = new StringBuilder();

            if (value is IEnumerable<Filter> filters)
            {
                foreach (var filter in filters)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(filter.ChainToPrevious ? ";" : ",");
                    }

                    AppendFilter(builder, filter);
                }

                return base.Convert(argName, builder.ToString() ?? string.Empty);
            }

            throw new NotSupportedException($"{nameof(value)} must be of type IEnumerable<Filter>.");
        }

        void AppendFilter(StringBuilder builder, Filter filter)
        {
            if (filter.Inputs != null)
            {
                foreach (var input in filter.Inputs)
                {
                    builder.Append($"[{input?.Specifier}]");
                }
            }

            builder.Append(filter.Name);

            if (filter.Options != null)
            {
                var optionBuilder = new StringBuilder();

                foreach (var option in filter.Options)
                {
                    if (optionBuilder.Length > 0)
                    {
                        optionBuilder.Append(":");
                    }

                    if (!string.IsNullOrWhiteSpace(option?.Name))
                    {
                        optionBuilder.Append($"{option.Name}=");
                    }

                    if (!string.IsNullOrWhiteSpace(option?.Value))
                    {
                        optionBuilder.Append(option.Value);
                    }
                }

                if (optionBuilder.Length > 0)
                {
                    builder.Append($"={optionBuilder}");
                }
            }

            if (filter.OutputLabels != null)
            {
                foreach (var label in filter.OutputLabels)
                {
                    builder.Append($"[{label}]");
                }
            }
        }
    }
}
