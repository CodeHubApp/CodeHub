using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using GitHubSharp;

namespace CodeHub.ViewControllers
{
	public class RawContentViewController : FileSourceViewController
    {
        private readonly string _rawUrl;

        public RawContentViewController(string rawUrl)
        {
            _rawUrl = rawUrl;
            Title = rawUrl.Substring(rawUrl.LastIndexOf('/') + 1);
        }

        protected override void Request()
        {
            try 
            {
                string mime = "text";
                var file = DownloadFile(_rawUrl, out mime);
                var ext = System.IO.Path.GetExtension(_rawUrl).TrimStart('.');
                if (mime.StartsWith("text"))
                    LoadRawData(System.Security.SecurityElement.Escape(System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8)), ext);
                else
                    LoadFile(file);
            }
            catch (InternalServerException ex)
            {
                MonoTouch.Utilities.ShowAlert("Error", ex.Message);
            }
        }
    }

    public class SourceInfoViewController : FileSourceViewController
    {
        protected string _user, _slug, _branch, _path;

        public SourceInfoViewController(string user, string slug, string branch, string path)
        {
            _user = user;
            _slug = slug;
            _branch = branch;
            _path = path;

            //Create the filename
            var fileName = System.IO.Path.GetFileName(path);
            if (fileName == null)
                fileName = path.Substring(path.LastIndexOf('/') + 1);

            //Create the temp file path
            Title = fileName;
        }

        protected override void Request()
        {
            try 
            {
                string mime = "text";
                var file = DownloadFile(_user, _slug, _branch, _path, out mime);
                var ext = System.IO.Path.GetExtension(_path).TrimStart('.');
                if (mime.StartsWith("text"))
                    LoadRawData(System.Security.SecurityElement.Escape(System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8)), ext);
                else
                    LoadFile(file);
            }
            catch (InternalServerException ex)
            {
                MonoTouch.Utilities.ShowAlert("Error", ex.Message);
            }
        }
    }

//    public class ContentViewController : FileSourceViewController
//    {
//        private readonly string _filename;
//        private readonly string _content;
//        private readonly string _encoding;
//
//        public ContentViewController(string filename, string content, string encoding)
//        {
//            _filename = filename;
//            _content = content;
//            _encoding = encoding;
//
//            Title = filename;
//        }
//
//        protected override void Request()
//        {
//            try 
//            {
//                var filename = Environment.TickCount + _filename;
//                var filepath = System.IO.Path.Combine(TempDir, filename);
//                string decoded;
//
//                if (string.Equals(_encoding, "base64", StringComparison.OrdinalIgnoreCase))
//                {
//                    var d = System.Convert.FromBase64String(_content);
//                    System.IO.File.WriteAllBytes(filepath, d);
//                }
//
//                LoadFile(filepath);
//            }
//            catch (InternalServerException ex)
//            {
//                MonoTouch.Utilities.ShowAlert("Error", ex.Message);
//            }
//        }
//    }
}

