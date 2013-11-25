using System;
using CodeHub.Core.ViewModels;
using System.Threading.Tasks;
using GitHubSharp.Models;
using CodeFramework.Core.ViewModels;

namespace CodeHub.Core
{
	public class GistFileViewModel : FileSourceViewModel
    {
		public GistFileModel GistFileModel { get; private set; }

		public void Init(NavObject navObject)
        {
			GistFileModel = navObject.GistFileModel;
        }

		protected override async Task Load(bool forceCacheInvalidation)
		{
			if (GistFileModel.Content != null)
			{
				var content = GistFileModel.Content;
				var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(GistFileModel.Filename));
				System.IO.File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
				FilePath = filePath;
				ContentPath = CreateContentFile();
			}
		}

		public class NavObject
		{
			public GistFileModel GistFileModel { get; set; }
		}
    }
}

