using System;
using System.Collections.Generic;
using System.Linq;
using Tricycle.Models;
using Tricycle.Models.Config;
using Xamarin.Forms;

namespace Tricycle.UI.Controls
{
    public partial class AudioConfigSection : ContentView
    {
        class Preset
        {
            public IList<AudioFormat> FormatOptions { get; } =
                Enum.GetValues(typeof(AudioFormat)).Cast<AudioFormat>().ToList();
            public IList<AudioMixdown> MixdownOptions { get; } =
                Enum.GetValues(typeof(AudioMixdown)).Cast<AudioMixdown>().ToList();
            public AudioFormat SelectedFormat { get; set; }
            public AudioMixdown SelectedMixdown { get; set; }
            public decimal Quality { get; set; }
        }

        public AudioConfigSection()
        {
            InitializeComponent();

            vwPresets.ItemsSource = new Preset[]
            {
                new Preset
                {
                    SelectedFormat = AudioFormat.Aac,
                    SelectedMixdown = AudioMixdown.Mono,
                    Quality = 96
                },
                new Preset
                {
                    SelectedFormat = AudioFormat.Aac,
                    SelectedMixdown = AudioMixdown.Stereo,
                    Quality = 160
                },
                new Preset
                {
                    SelectedFormat = AudioFormat.Aac,
                    SelectedMixdown = AudioMixdown.Surround5dot1,
                    Quality = 640
                },
                new Preset
                {
                    SelectedFormat = AudioFormat.Ac3,
                    SelectedMixdown = AudioMixdown.Mono,
                    Quality = 96
                },
                new Preset
                {
                    SelectedFormat = AudioFormat.Ac3,
                    SelectedMixdown = AudioMixdown.Stereo,
                    Quality = 160
                },
                new Preset
                {
                    SelectedFormat = AudioFormat.Ac3,
                    SelectedMixdown = AudioMixdown.Surround5dot1,
                    Quality = 640
                }
            };
        }
    }
}
