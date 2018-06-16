using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using System.Threading.Tasks;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.User;
using System.Reactive;
using Splat;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Commits
{
    public class CommitViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IFeaturesService _featuresService;

        public string Node { get; }

        public string User { get; }

        public string Repository { get; }

        public bool ShowRepository { get; }

        private Octokit.GitHubCommit _commitModel;
        public Octokit.GitHubCommit Changeset
        {
            get { return _commitModel; }
            private set { this.RaiseAndSetIfChanged(ref _commitModel, value); }
        }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        public ReactiveCommand<Unit, RepositoryViewModel> GoToRepositoryCommand { get; }

        public ReactiveCommand<Unit, string> GoToHtmlUrlCommand { get; }

        public ReactiveList<Octokit.CommitComment> Comments { get; } = new ReactiveList<Octokit.CommitComment>();

        public ReactiveCommand<Unit, UserViewModel> GoToOwner { get; }

        public CommitViewModel(
            string username,
            string repository,
            string node,
            bool showRepository = false,
            IApplicationService application = null,
            IFeaturesService featuresService = null)
        {
            User = username;
            Repository = repository;
            Node = node;
            ShowRepository = showRepository;

            _applicationService = application ?? Locator.Current.GetService<IApplicationService>();
            _featuresService = featuresService ?? Locator.Current.GetService<IFeaturesService>();

            GoToRepositoryCommand = ReactiveCommand.Create(
                () => new RepositoryViewModel(User, Repository));

            GoToHtmlUrlCommand = ReactiveCommand.Create(
                () => Changeset.HtmlUrl,
                this.WhenAnyValue(x => x.Changeset).Select(x => x != null));

            GoToOwner = ReactiveCommand.Create(
                () => new UserViewModel(Changeset.Author.Login),
                this.WhenAnyValue(x => x.Changeset.Author.Login).Select(x => x != null));

            Title = "Commit " + (Node.Length > 6 ? Node.Substring(0, 6) : Node);
        }

        protected override Task Load()
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
            {
                _applicationService.GitHubClient.Repository.Get(User, Repository)
                    .ToBackground(x => ShouldShowPro = x.Private && !_featuresService.IsProEnabled);
            }

            var commit = _applicationService.GitHubClient.Repository.Commit.Get(User, Repository, Node);

            _applicationService
                .GitHubClient.Repository.Comment.GetAllForCommit(User, Repository, Node)
                .ToBackground(Comments.Reset);

            return commit;
        }

        public async Task AddComment(string text)
        {
            var result = await _applicationService.GitHubClient.Repository.Comment.Create(
                User, Repository, Node, new Octokit.NewCommitComment(text));
            Comments.Add(result);
        }
    }
}

