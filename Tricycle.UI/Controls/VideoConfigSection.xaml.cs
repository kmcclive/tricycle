using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Tricycle.UI.Controls
{
    public partial class VideoConfigSection : ContentView
    {
        class Preset
        {
            public string Name { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public VideoConfigSection()
        {
            InitializeComponent();

            vwSizePresets.ItemsSource = new Preset[]
            {
                new Preset()
                {
                    Name = "4K",
                    Width = 3840,
                    Height = 2160
                },
                new Preset()
                {
                    Name = "1080p",
                    Width = 1920,
                    Height = 1080
                },
                new Preset()
                {
                    Name = "720p",
                    Width = 1280,
                    Height = 720
                },
                new Preset()
                {
                    Name = "480p",
                    Width = 853,
                    Height = 480
                }
            };

            vwAspectRatioPresets.ItemsSource = new Preset[]
            {
                new Preset()
                {
                    Name = "21:9",
                    Width = 21,
                    Height = 9
                },
                new Preset()
                {
                    Name = "16:9",
                    Width = 16,
                    Height = 9
                }
            };
        }
    }
}
