using System;
using System.Text;
using Tricycle.Media.FFmpeg.Models.Jobs;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public class CodecConverter : ArgumentConverter
    {
        public override string Convert(string argName, object value)
        {
            if (value is Codec codec)
            {
                if (string.IsNullOrWhiteSpace(codec.Name))
                {
                    throw new ArgumentException($"{nameof(codec.Name)} must not be null or empty.", nameof(value));
                }

                string options = null;

                if (codec.Options?.Count > 0)
                {
                    var builder = new StringBuilder();

                    foreach (var option in codec.Options)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(" ");
                        }

                        builder.Append($"-{option.Key}");

                        if (!string.IsNullOrWhiteSpace(option.Value))
                        {
                            builder.Append($" {option.Value}");
                        }
                    }

                    options = builder.ToString();
                }

                string valueString = !string.IsNullOrWhiteSpace(options) ? $"{codec.Name} {options}" : codec.Name;

                return base.Convert(argName, valueString);
            }

            throw new NotSupportedException($"{nameof(value)} must be of type Codec.");
        }
    }
}
