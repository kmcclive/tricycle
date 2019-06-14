using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricycle.IO;
using Tricycle.Models;
using Tricycle.ViewModels;
using Xamarin.Forms;

namespace Tricycle
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainViewModel(AppState.FileBrowser, AppState.MediaInspector, new TricycleConfig());
        }
    }
}
