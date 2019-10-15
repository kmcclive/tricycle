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
            if (e.NewElement?.Source is FileImageSource fileSource)
            {
                e.NewElement.Source = new FileImageSource()
                {
                    AutomationId = fileSource.AutomationId,
                    BindingContext = fileSource.BindingContext,
                    ClassId = fileSource.ClassId,
                    File = fileSource.File != null ? Path.Combine("Assets", fileSource.File) : null,
                    Parent = fileSource.Parent,
                    StyleId = fileSource.StyleId
                };
            }

            base.OnElementChanged(e);
        }
    }
}
