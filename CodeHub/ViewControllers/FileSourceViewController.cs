using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.Controllers;
using System.Text;
using CodeFramework.Controllers;
using CodeFramework.Views;

namespace CodeHub.ViewControllers
{
    public abstract class FileSourceViewController : CodeFramework.Controllers.FileSourceController
    {
        protected static string DownloadFile(string user, string slug, string branch, string path, out string mime)
        {
            //Create a temporary filename
            var ext = System.IO.Path.GetExtension(path);
            if (ext == null) ext = string.Empty;
            var filename = Environment.TickCount + ext;
            var filepath = System.IO.Path.Combine(TempDir, filename);

            //Find
            using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                mime = Application.Client.Users[user].Repositories[slug].GetFileRaw(branch, path, stream);
            }

            return filepath;
        }

        protected static string DownloadFile(string rawUrl, out string mime)
        {
            //Create a temporary filename
            var ext = System.IO.Path.GetExtension(rawUrl);
            if (ext == null) ext = string.Empty;
            var filename = Environment.TickCount + ext;
            var filepath = System.IO.Path.Combine(TempDir, filename);

            //Find
            using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                mime = Application.Client.DownloadRawResource(rawUrl, stream);
            }

            return filepath;
        }
    }
}

