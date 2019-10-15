using Tricycle.UI.Windows.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
namespace Tricycle.UI.Windows.Renderers
{
    public class CustomPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if ((Control != null) && (e.NewElement != null))
            {
                Control.FontSize = e.NewElement.FontSize;
            }
        }
    }
}
