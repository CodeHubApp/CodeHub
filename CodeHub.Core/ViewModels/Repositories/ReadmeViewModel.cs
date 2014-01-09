using System;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using System.Windows.Input;
using CodeFramework.Core.ViewModels;
using CodeFramework.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
	public class ReadmeViewModel : LoadableViewModel
    {
        private string _data;
        private string _path;
		private GitHubSharp.Models.ContentModel _contentModel;

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public string Data
        {
            get { return _data; }
            set { _data = value; RaisePropertyChanged(() => Data); }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; RaisePropertyChanged(() => Path); }
        }

		public ICommand GoToGitHubCommand
		{
			get { return new MvxCommand(() => GoToUrlCommand.Execute(_contentModel.HtmlUrl), () => _contentModel != null); }
		}

		public ICommand GoToLinkCommand
		{
			get { return GoToUrlCommand; }
		}

		public ICommand ShareCommand
		{
			get
			{
				return new MvxCommand(() => GetService<IShareService>().ShareUrl(_contentModel.HtmlUrl), () => _contentModel != null);
			}
		}

		protected override async Task Load(bool forceCacheInvalidation)
		{
			var wiki = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].GetReadme());
			_contentModel = wiki.Data;
			var d = Encoding.UTF8.GetString(Convert.FromBase64String(wiki.Data.Content));
			Data = await this.GetApplication().Client.Markdown.GetMarkdown(d);
			Path = CreateHtmlFile(Data);
		}
		
        private string CreateHtmlFile(string data)
        {
            //Generate the markup
			var markup = System.IO.File.ReadAllText("Markdown/markdown.html", Encoding.UTF8);

            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName() + ".html");
            using (var tmpStream = new System.IO.FileStream(tmp, System.IO.FileMode.Create))
            {
				var fs = new System.IO.StreamWriter(tmpStream, Encoding.UTF8);
                var dataIndex = markup.IndexOf("{{DATA}}", StringComparison.Ordinal);
                fs.Write(markup.Substring(0, dataIndex));
                fs.Write(data);
                fs.Write(markup.Substring(dataIndex + 8));
                fs.Flush();
            }
            return tmp;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}
