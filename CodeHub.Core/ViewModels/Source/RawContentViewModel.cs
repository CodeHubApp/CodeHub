using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHubSharp;

namespace CodeHub.Core.ViewModels.Source
{
    public class RawContentViewModel : LoadableViewModel
    {
        public string RawUrl { get; private set; }

        public string GitHubUrl { get; private set; }

        public void Init(NavObject navObject)
        {
            RawUrl = navObject.RawUrl;
            GitHubUrl = navObject.GitHubUrl;
        }

        protected override Task Load(bool forceCacheInvalidation)
        {
//            try
//            {
//                var result = _downloadResult = DownloadFile(RawUrl);
//                var ext = System.IO.Path.GetExtension(_rawUrl).TrimStart('.');
//                if (!result.IsBinary)
//                    LoadRawData(System.Security.SecurityElement.Escape(System.IO.File.ReadAllText(result.File, System.Text.Encoding.UTF8)), ext);
//                else
//                    LoadFile(result.File);
//            }
//            catch (InternalServerException ex)
//            {
//                MonoTouch.Utilities.ShowAlert("Error", ex.Message);
//            }
            return null;
        }


        public class DownloadResult
        {
            public string File { get; set; }
            public bool IsBinary { get; set; }
        }

        protected static string CreateFile(string filename)
        {
            var ext = System.IO.Path.GetExtension(filename);
            if (ext == null) ext = string.Empty;
            var newFilename = Environment.TickCount + ext;
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), newFilename);
        }

        protected DownloadResult DownloadFile(string rawUrl)
        {
            //Create a temporary filename
            var filepath = CreateFile(rawUrl);
            string mime = null;

            //Find
            using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                mime = Application.Client.DownloadRawResource(rawUrl, stream) ?? string.Empty;
            }

            return new DownloadResult { IsBinary = IsBinary(mime), File = filepath };
        }

        private static readonly string[] BinaryMIMEs =
        { 
            "image/", "video/", "audio/", "model/", "application/pdf", "application/zip", "application/gzip"
        };

        private static bool IsBinary(string mime)
        {
            var lowerMime = mime.ToLower();
            return BinaryMIMEs.Any(lowerMime.StartsWith);
        }

        public class NavObject
        {
            public string RawUrl { get; set; }
            public string GitHubUrl { get; set; }
        }
    }
}
