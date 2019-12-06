using System;
namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class LabeledInput : IInput
    {
        public string Label { get; set; }
        public string Specifier => Label;

        public LabeledInput()
        {

        }

        public LabeledInput(string label)
        {
            Label = label;
        }
    }
}
