using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class ReadmeViewModel : BaseViewModel, ILoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

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
	        private set { this.RaiseAndSetIfChanged(ref _contentModel, value); }
	    }

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToGitHubCommand { get; private set; }

        public IReactiveCommand<object> GoToLinkCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public ReadmeViewModel(IApplicationService applicationService, IShareService shareService)
        {
            Title = "Readme";

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ContentModel).Select(x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(ContentModel.HtmlUrl));

            GoToGitHubCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ContentModel).Select(x => x != null));
            GoToGitHubCommand.Select(_ => ContentModel.HtmlUrl).Subscribe(this.ShowWebBrowser);

            GoToLinkCommand = ReactiveCommand.Create();
            GoToLinkCommand.OfType<string>().Subscribe(this.ShowWebBrowser);

            LoadCommand = ReactiveCommand.CreateAsyncTask(async x =>
            {
                var repository = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName];
                ContentText = await repository.GetReadmeRendered();
                ContentModel = (await applicationService.Client.ExecuteAsync(repository.GetReadme())).Data;
            });
        }
    }
}
