using System;
using CodeBucket.Bitbucket.Controllers;

namespace CodeBucket.GitHub.Controllers
{
    public abstract class FileViewController : FileSourceController
    {
        protected FileViewController()
        {
        }

        protected new static string DownloadFile(string user, string slug, string branch, string path, out string mime)
        {
            //Create a temporary filename
            var ext = System.IO.Path.GetExtension(path);
            if (ext == null) ext = string.Empty;
            var filename = Environment.TickCount + ext;
            var filepath = System.IO.Path.Combine(TempDir, filename);
            
            //Find
            using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var response = Application.GitHubClient.API.GetFileRaw(user, slug, branch, path, stream);
                mime = response.ContentType;
            }
            
            return filepath;
        }
    }
}

