using System;
using System.Collections.Generic;
using System.Text;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg
{
    public class FFmpegArgumentGenerator : IFFmpegArgumentGenerator
    {
        #region Fields

        readonly IArgumentPropertyReflector _reflector;

        #endregion

        #region Constructors

        public FFmpegArgumentGenerator(IArgumentPropertyReflector reflector)
        {
            _reflector = reflector;
        }

        #endregion

        #region Methods

        public string GenerateArguments(FFmpegJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (string.IsNullOrWhiteSpace(job.InputFileName))
            {
                throw new ArgumentException($"{nameof(job)}.{nameof(job.InputFileName)} is null or empty.", nameof(job));
            }

            IEnumerable<ArgumentProperty> properties = _reflector.Reflect(job);

            var builder = new StringBuilder();

            foreach (var property in properties)
            {
                if (builder.Length > 0)
                {
                    builder.Append(" ");
                }

                string argument = property?.Converter?.Convert(property?.ArgumentName, property?.Value);

                if (!string.IsNullOrWhiteSpace(argument))
                {
                    builder.Append(argument);
                }
            }

            if (string.IsNullOrWhiteSpace(job.OutputFileName) && string.IsNullOrWhiteSpace(job.Format))
            {
                // Some jobs like crop or interlace detection won't write to a file,
                // but FFmpeg still requires the output argument
                builder.Append(" -f null -");
            }

            return builder?.ToString();
        }

        #endregion
    }
}
