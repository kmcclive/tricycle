using System.Threading.Tasks;
using System.Windows.Forms;
using Tricycle.IO.Models;

namespace Tricycle.IO.Windows
{
    public class FolderBrowser : IFolderBrowser
    {
        public Task<FolderBrowserResult> Browse()
        {
            return Browse(null);
        }

        public Task<FolderBrowserResult> Browse(string defaultDirectory)
        {
            var result = new FolderBrowserResult();

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = defaultDirectory;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    result.Confirmed = true;
                    result.FolderName = dialog.SelectedPath;
                }
            }

            return Task.FromResult(result);
        }
    }
}
