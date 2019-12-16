using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class MacTitleBar : ContentView
    {
        public static readonly BindableProperty StatusProperty = BindableProperty.Create(
          nameof(Status),
          typeof(string),
          typeof(MacTitleBar));
        public static readonly BindableProperty PreviewCommandProperty = BindableProperty.Create(
          nameof(PreviewCommand),
          typeof(ICommand),
          typeof(MacTitleBar));

        public string Status
        {
            get { return GetValue(StatusProperty)?.ToString(); }
            set { SetValue(StatusProperty, value); }
        }

        public ICommand PreviewCommand
        {
            get { return (ICommand)GetValue(PreviewCommandProperty); }
            set { SetValue(PreviewCommandProperty, value); }
        }

        public MacTitleBar()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Status):
                    lblStatus.Text = Status;
                    break;
            }
        }
    }
}
