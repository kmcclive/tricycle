using AppKit;

namespace Tricycle.UI.macOS
{
    public class AppDocumentController : NSDocumentController
    {
        IAppManager _appManager;
        volatile bool _isBusy;

        public AppDocumentController(IAppManager appManager)
        {
            _appManager = appManager;

            _appManager.Busy += () =>
            {
                _isBusy = true;
            };
            _appManager.Ready += () =>
            {
                _isBusy = false;
            };
        }

        public override bool ValidateMenuItem(NSMenuItem menuItem)
        {
            switch (menuItem.ParentItem?.Title)
            {
                case "Open Recent":
                    return !_isBusy || menuItem.Title == "Clear Menu";
                default:
                    return true;
            }
        }
    }
}
