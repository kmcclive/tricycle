using System;
using System.Collections.Generic;

namespace Tricycle.Media.FFmpeg.Serialization.Argument
{
    public interface IArgumentPropertyReflector
    {
        IList<ArgumentProperty> Reflect(object obj);
    }
}
