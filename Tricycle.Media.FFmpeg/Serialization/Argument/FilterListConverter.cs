using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Tricycle.Media.FFmpeg.Models.Jobs;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class FilterListConverter : ArgumentConverter, IArgumentConverter
    {
        IArgumentConverter _optionListConverter = new OptionListConverter();

        public override string Convert(string argName, object value)
        {
            var builder = new StringBuilder();

            if (value is IEnumerable<IFilter> filters)
            {
                foreach (var filter in filters)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(filter.ChainToPrevious ? ";" : ",");
                    }

                    switch (filter)
                    {
                        case CustomFilter customFilter:
                            AppendCustomFilter(builder, customFilter);
                            break;
                        case Filter normalFilter:
                            AppendFilter(builder, normalFilter);
                            break;
                    }
                }

                return base.Convert(argName, builder.ToString() ?? string.Empty);
            }

            throw new NotSupportedException($"{nameof(value)} must be of type IEnumerable<IFilter>.");
        }

        void AppendCustomFilter(StringBuilder builder, CustomFilter filter)
        {
            builder.Append(filter.Data);
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
                var optionString = _optionListConverter.Convert(string.Empty, filter.Options)?.Trim();

                if (optionString.Length > 0)
                {
                    builder.Append($"={optionString}");
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
