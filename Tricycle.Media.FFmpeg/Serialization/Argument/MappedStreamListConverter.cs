using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class MappedStreamListConverter : ArgumentConverter
    {
        public override string Convert(string argName, object value)
        {
            int audioCount = 0, videoCount = 0, subtitleCount = 0;

            if (value is IEnumerable<MappedStream> streams)
            {
                var builder = new StringBuilder();

                foreach (var stream in streams)
                {
                    if (stream == null)
                    {
                        continue;
                    }

                    if (builder.Length > 0)
                    {
                        builder.Append(" ");
                    }

                    if (stream.Input == null)
                    {
                        throw new ArgumentException($"{nameof(stream.Input)} must not be null.");
                    }

                    builder.Append($"{argName} {stream.Input.Specifier}");

                    int typeCount;
                    string type;

                    switch(stream.StreamType)
                    {
                        case StreamType.Audio:
                            typeCount = audioCount;
                            type = "a";
                            audioCount++;
                            break;
                        case StreamType.Video:
                            typeCount = videoCount;
                            type = "v";
                            videoCount++;
                            break;
                        case StreamType.Subtitle:
                            typeCount = subtitleCount;
                            type = "s";
                            subtitleCount++;
                            break;
                        default:
                            throw new NotSupportedException($"Stream type {stream.StreamType} is not supported.");
                    }

                    string options = GetOptions(stream, $"{type}:{typeCount}");

                    if (options.Length > 0)
                    {
                        builder.Append($" {options}");
                    }
                }

                return builder.ToString() ?? string.Empty;
            }

            throw new NotSupportedException($"{nameof(value)} must be of type IEnumerable<MappedStream>");
        }

        string GetOptions(MappedStream stream, string outputSpecifier)
        {
            var optionBuilder = new StringBuilder();

            foreach (var property in stream.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var argument = property.GetCustomAttribute<ArgumentAttribute>();
                var propValue = property.GetValue(stream);

                if ((argument != null) && (property != null))
                {
                    if (optionBuilder.Length > 0)
                    {
                        optionBuilder.Append(" ");
                    }

                    optionBuilder.Append($"{argument.Name}:{outputSpecifier} {propValue}");
                }
            }

            return optionBuilder.ToString();
        }
    }
}
