using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Tricycle.UI.Views
{
    public partial class NavigationBar : ContentView
    {
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
          nameof(Title),
          typeof(string),
          typeof(NavigationBar));

        public static readonly BindableProperty BackCommandProperty = BindableProperty.Create(
          nameof(BackCommand),
          typeof(ICommand),
          typeof(NavigationBar));

        public string Title
        {
            get { return GetValue(TitleProperty).ToString(); }
            set { SetValue(TitleProperty, value); }
        }

        public ICommand BackCommand
        {
            get { return (ICommand)GetValue(BackCommandProperty); }
            set { SetValue(BackCommandProperty, value); }
        }

        public NavigationBar()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(BackCommand):
                    btnBack.Command = BackCommand;
                    break;
                case nameof(Title):
                    lblTitle.Text = Title;
                    break;
            }
        }
    }
}
