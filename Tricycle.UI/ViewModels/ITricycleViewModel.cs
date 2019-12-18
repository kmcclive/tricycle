using System.Windows.Input;

namespace Tricycle.UI.ViewModels
{
    public interface ITricycleViewModel
    {
        bool IsSpinnerVisible { get; }

        string Status { get; }

        double Progress { get; }

        bool IsBackVisible { get; }

        ICommand BackCommand { get; }

        bool IsPreviewVisible { get; }

        ICommand PreviewCommand { get; }

        bool IsStartVisible { get; }

        ICommand StartCommand { get; }

        string StartImageSource { get; }
    }
}
