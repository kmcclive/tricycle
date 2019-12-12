using AppKit;

namespace Tricycle.UI.macOS
{
    public class AppDocumentController : NSDocumentController
    {
        IAppManager _appManager;

        public AppDocumentController(IAppManager appManager)
        {
            _appManager = appManager;
        }

        public override bool ValidateMenuItem(NSMenuItem menuItem)
        {
            switch (menuItem.ParentItem?.Title)
            {
                case "Open Recent":
                    return !(_appManager.IsBusy || _appManager.IsModalOpen) || menuItem.Title == "Clear Menu";
                default:
                    return true;
            }
        }
    }
}
