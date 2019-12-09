using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public interface IArgumentConverter
    {
        IArgumentPropertyReflector Reflector { get; set; }

        string Convert(string argName, object value);
    }
}
