using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tricycle.UI.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;

            if (propertyName != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
