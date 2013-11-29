using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using GitHubSharp.Models;
using System;
using CodeFramework.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
	public class SourceViewModel : FileSourceViewModel
    {
		private string _path;
		private string _name;
		private string _gitUrl;
		private bool _forceBinary;

		protected override async Task Load(bool forceCacheInvalidation)
        {
			var filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(_name));

			var mime = await Task.Run<string>(() =>
			{
				using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
				{
					return this.GetApplication().Client.DownloadRawResource2(_gitUrl, stream) ?? string.Empty;
				}
			});

			FilePath = filepath;

			// We can force a binary representation if it was passed during init. In which case we don't care to figure out via the mime.
			if (_forceBinary)
				return;

			var isText = mime.Contains("charset");
			if (isText)
			{
				ContentPath = CreateContentFile();
			}
        }

		public void Init(NavObject navObject)
		{
			_path = navObject.Path;
			HtmlUrl = navObject.HtmlUrl;
			_name = navObject.Name;
			_gitUrl = navObject.GitUrl;
			_forceBinary = navObject.ForceBinary;

			//Create the filename
			var fileName = System.IO.Path.GetFileName(_path);
			if (fileName == null)
				fileName = _path.Substring(_path.LastIndexOf('/') + 1);

			//Create the temp file path
			Title = fileName;
		}

		public class NavObject
		{
			public string Path { get; set; }
			public string HtmlUrl { get; set; }
			public string Name { get; set; }
			public string GitUrl { get; set; }
			public bool ForceBinary { get; set; }
		}
    }
}