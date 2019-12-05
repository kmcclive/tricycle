using System;
namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public interface IArgumentConverter
    {
        string Convert(string argName, object value);
    }
}
