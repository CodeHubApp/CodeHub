using System;
using CodeHub.Core.ViewModels;
using System.Threading.Tasks;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Services;

namespace CodeHub.Core
{
	public class GistFileViewModel : FileSourceViewModel
    {
		private string _id;
		private string _filename;
		private GistFileModel _fileModel;

		public void Init(NavObject navObject)
        {
			//Create the filename
			var fileName = System.IO.Path.GetFileName(navObject.Filename);
			if (fileName == null)
				fileName = navObject.Filename.Substring(navObject.Filename.LastIndexOf('/') + 1);

			//Create the temp file path
			Title = fileName;

			_id = navObject.GistId;
			_filename = navObject.Filename;

			//Grab the data
			_fileModel = GetService<IViewModelTxService>().Get() as GistFileModel;
        }

		protected override async Task Load(bool forceCacheInvalidation)
		{
			if (_fileModel == null)
			{
				var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Gists[_id].Get());
				_fileModel = data.Data.Files[_filename];
			}

			//Check to make sure...
			if (_fileModel == null || _fileModel.Content == null)
			{
				throw new Exception("Unable to retreive gist!");
			}

			var content = _fileModel.Content;
			var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(_fileModel.Filename));
			System.IO.File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
			FilePath = filePath;
			ContentPath = CreateContentFile();
		}

		public class NavObject
		{
			public string GistId { get; set; }
			public string Filename { get; set; }
		}
    }
}

