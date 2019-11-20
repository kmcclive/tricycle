using System.ComponentModel;
using System.IO;
using Tricycle.UI.Windows.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportRenderer(typeof(ImageButton), typeof(CustomImageButtonRenderer))]
namespace Tricycle.UI.Windows.Renderers
{
    public class CustomImageButtonRenderer : ImageButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
        {
            CorrectSource(e.NewElement);
            
            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var button = (ImageButton)sender;

            if (e.PropertyName == nameof(button.Source))
            {
                CorrectSource(button);
            }

            base.OnElementPropertyChanged(sender, e);
        }

        protected void CorrectSource(ImageButton button)
        {
            if (button?.Source is FileImageSource fileSource && fileSource.File != null && !fileSource.File.StartsWith("Assets"))
            {
                button.Source = new FileImageSource()
                {
                    AutomationId = fileSource.AutomationId,
                    BindingContext = fileSource.BindingContext,
                    ClassId = fileSource.ClassId,
                    File = Path.Combine("Assets", fileSource.File),
                    Parent = fileSource.Parent,
                    StyleId = fileSource.StyleId
                };
            }
        }
    }
}
