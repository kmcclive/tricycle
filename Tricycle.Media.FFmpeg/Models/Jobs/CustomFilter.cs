using System;
namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class CustomFilter : IFilter
    {
        public string Data { get; set; }
        public bool ChainToPrevious => false;

        public CustomFilter()
        {

        }

        public CustomFilter(string data)
        {
            Data = data;
        }
    }
}
