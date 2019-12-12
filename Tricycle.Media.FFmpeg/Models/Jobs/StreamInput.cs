using System;
namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class StreamInput : IInput
    {
        public int FileIndex { get; set; }
        public int StreamIndex { get; set; }
        public string Specifier => $"{FileIndex}:{StreamIndex}";

        public StreamInput()
        {

        }

        public StreamInput(int fileIndex, int streamIndex)
        {
            FileIndex = fileIndex;
            StreamIndex = streamIndex;
        }
    }
}
