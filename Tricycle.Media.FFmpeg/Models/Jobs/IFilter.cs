using System;
namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public interface IFilter
    {
        bool ChainToPrevious { get; }
    }
}
