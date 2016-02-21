using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class ReadmeViewModel : LoadableViewModel
    {
        private readonly IMarkdownService _markdownService;
        private string _data;
        private string _path;
        private GitHubSharp.Models.ContentModel _contentModel;

        public string Username 
        {
            get;
            private set; 
        }

        public string Repository 
        {
            get;
            private set; 
        }

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

        public string HtmlUrl
        {
            get { return _contentModel?.HtmlUrl; }
        }

        public ICommand GoToGitHubCommand
        {
            get { return new MvxCommand(() => GoToUrlCommand.Execute(HtmlUrl), () => _contentModel != null); }
        }

        public ICommand GoToLinkCommand
        {
            get { return GoToUrlCommand; }
        }

        public ReadmeViewModel(IMarkdownService markdownService)
        {
            _markdownService = markdownService;
        }

        protected override Task Load(bool forceCacheInvalidation)
        {
            return this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].GetReadme(), forceCacheInvalidation, x =>
            {
                _contentModel = x.Data;
                var data = _markdownService.Convert(Encoding.UTF8.GetString(Convert.FromBase64String(x.Data.Content)));
                Path = MarkdownHtmlGenerator.CreateFile(data);
            });
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
