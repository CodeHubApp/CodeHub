using System;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.Services;
using ReactiveUI;
using GitHubSharp.Models;
using Xamarin.Utilities.Core.Services;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleaseViewModel : BaseViewModel, ILoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public long ReleaseId { get; set; }

        private readonly ObservableAsPropertyHelper<string> _contentText;
        public string ContentText { get { return _contentText.Value; } }

        private ReleaseModel _releaseModel;
        public ReleaseModel ReleaseModel
        {
            get { return _releaseModel; }
            set { this.RaiseAndSetIfChanged(ref _releaseModel, value); }
        }

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToGitHubCommand { get; private set; }

        public IReactiveCommand<object> GoToLinkCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public ReleaseViewModel(IApplicationService applicationService, IMarkdownService markdownService, IShareService shareService)
        {
            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(ReleaseModel.HtmlUrl));

            GoToGitHubCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null));
            GoToGitHubCommand.Subscribe(_ => GoToUrlCommand.ExecuteIfCan(ReleaseModel.HtmlUrl));

            GoToLinkCommand = ReactiveCommand.Create();
            GoToLinkCommand.OfType<string>().Subscribe(x => GoToUrlCommand.ExecuteIfCan(x));

            _contentText = this.WhenAnyValue(x => x.ReleaseModel).IsNotNull()
                .Select(x => markdownService.Convert(x.Body)).ToProperty(this, x => x.ContentText);

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => 
                this.RequestModel(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetRelease(ReleaseId), 
                    x as bool?, r => ReleaseModel = r.Data));
        }
    }
}

