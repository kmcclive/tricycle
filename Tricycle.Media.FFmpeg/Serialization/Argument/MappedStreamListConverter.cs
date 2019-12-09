using System;
using System.Collections.Generic;
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

            Append(optionBuilder, stream, outputSpecifier, 0);

            return optionBuilder.ToString();
        }

        void Append(StringBuilder builder, object obj, string outputSpecifier, int level)
        {
            foreach (var property in Reflector.Reflect(obj))
            {
                if (builder.Length > 0)
                {
                    builder.Append(" ");
                }

                var type = property.Value.GetType();
                string argName = property.ArgumentName;

                if (level == 0)
                {
                    argName += $":{outputSpecifier}";
                }

                if (IsSimpleType(type) || IsSimpleType(Nullable.GetUnderlyingType(type)))
                {
                    builder.Append(property.Converter.Convert(argName, property.Value)?.Trim());
                }
                else
                {
                    builder.Append(argName);
                    Append(builder, property.Value, outputSpecifier, level + 1);
                }
            }
        }

        bool IsSimpleType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return type.IsPrimitive
            || type.IsEnum
            || type.Equals(typeof(string))
            || type.Equals(typeof(decimal));
        }
    }
}
