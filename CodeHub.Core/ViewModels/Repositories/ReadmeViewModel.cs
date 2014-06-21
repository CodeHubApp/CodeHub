using System;
using System.Reactive.Linq;
using System.Text;
using CodeFramework.Core.Services;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Repositories
{
	public class ReadmeViewModel : LoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private string _contentText;
        public string ContentText
        {
            get { return _contentText; }
            set { this.RaiseAndSetIfChanged(ref _contentText, value); }
        }

	    private ContentModel _contentModel;
	    public ContentModel ContentModel
	    {
	        get { return _contentModel; }
	        private set { this.RaiseAndSetIfChanged(ref _contentModel, value); }
	    }

		public IReactiveCommand GoToGitHubCommand { get; private set; }

		public IReactiveCommand GoToLinkCommand { get; private set; }

		public IReactiveCommand ShareCommand { get; private set; }

        public ReadmeViewModel(IApplicationService applicationService, IMarkdownService markdownService, IShareService shareService)
        {
            ShareCommand = new ReactiveCommand(this.WhenAnyValue(x => x.ContentModel, x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(ContentModel.HtmlUrl));

            GoToGitHubCommand = new ReactiveCommand(this.WhenAnyValue(x => x.ContentModel, x => x != null));
            GoToGitHubCommand.Subscribe(_ => GoToUrlCommand.ExecuteIfCan(ContentModel.HtmlUrl));

            GoToLinkCommand = new ReactiveCommand();
            GoToLinkCommand.OfType<string>().Subscribe(x => GoToUrlCommand.ExecuteIfCan(x));

            LoadCommand.RegisterAsyncTask(x => 
                this.RequestModel(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetReadme(), x as bool?, r =>
                {
                    ContentModel = r.Data;
                    ContentText = markdownService.Convert(Encoding.UTF8.GetString(Convert.FromBase64String(ContentModel.Content)));
                }));
        }
    }
}
