using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Services;
using MvvmCross.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class ReadmeViewModel : LoadableViewModel
    {
        private readonly IMarkdownService _markdownService;

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        private string _contentText;
        public string ContentText
        {
            get { return _contentText; }
            private set { this.RaiseAndSetIfChanged(ref _contentText, value); }
        }

        private ContentModel _contentModel;
        public ContentModel ContentModel
        {
            get { return _contentModel; }
            set { this.RaiseAndSetIfChanged(ref _contentModel, value); }
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
            Title = "Readme";
            _markdownService = markdownService;
        }

        protected override async Task Load()
        {
            var cmd = this.GetApplication().Client.Users[RepositoryOwner].Repositories[RepositoryName].GetReadme();
            var result = await this.GetApplication().Client.ExecuteAsync(cmd);
            ContentModel = result.Data;
            ContentText = _markdownService.Convert(Encoding.UTF8.GetString(Convert.FromBase64String(result.Data.Content)));
        }

        public void Init(NavObject navObject)
        {
            RepositoryOwner = navObject.Username;
            RepositoryName = navObject.Repository;
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}
