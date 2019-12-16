using System;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Tricycle.UI.macOS.Renderers;
using Tricycle.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(GradientStackLayout), typeof(GradientStackLayoutRenderer))]
namespace Tricycle.UI.macOS.Renderers
{
    public class GradientStackLayoutRenderer : VisualElementRenderer<StackLayout>
    {
        public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            var layout = (GradientStackLayout)this.Element;
            var gradient = new CAGradientLayer()
            {
                Frame = dirtyRect,
                BorderColor = layout.BorderColor.ToCGColor(),
                BorderWidth = (nfloat)layout.BorderWidth,
                Colors = layout.GradientColors?.Reverse().Select(c => c.ToCGColor()).ToArray()
            };

            if (layout.GradientOrientation == StackOrientation.Horizontal)
            {
                gradient.StartPoint = new CGPoint(0, 0.5);
                gradient.EndPoint = new CGPoint(1, 0.5);
            }
            else
            {
                gradient.StartPoint = new CGPoint(0.5, 0);
                gradient.EndPoint = new CGPoint(0.5, 1);
            }

            NativeView.Layer.InsertSublayer(gradient, 0);
        }
    }
}
