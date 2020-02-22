using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Tricycle.UI.ViewModels
{
    public class TemplateViewModel : ViewModelBase
    {
        string _oldName;
        string _newName;

        public TemplateViewModel(string name)
            : this()
        {
            _oldName = name;
            _newName = name;
        }

        public TemplateViewModel()
        {
            RemoveCommand = new Command(() => Removed?.Invoke());
        }

        public string OldName
        {
            get => _oldName;
            set
            {
                if (value != _oldName)
                {
                    SetProperty(ref _oldName, value);
                    Modified?.Invoke();
                }
            }
        }

        public string NewName
        {
            get => _newName;
            set
            {
                if (value != _newName)
                {
                    SetProperty(ref _newName, value);
                    Modified?.Invoke();
                }
            }
        }

        public ICommand RemoveCommand { get; }

        public event Action Modified;
        public event Action Removed;

        public void ClearHandlers()
        {
            if (Modified != null)
            {
                foreach (Action handler in Modified.GetInvocationList())
                {
                    Modified -= handler;
                }
            }

            if (Removed != null)
            {
                foreach (Action handler in Removed.GetInvocationList())
                {
                    Removed -= handler;
                }
            }
        }
    }
}
