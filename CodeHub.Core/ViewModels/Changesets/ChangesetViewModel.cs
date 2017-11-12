using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Source;
using MvvmCross.Platform;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.User;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class ChangesetViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<CommentModel> _comments = new CollectionViewModel<CommentModel>();
        private readonly IApplicationService _applicationService;
        private readonly IFeaturesService _featuresService;
        private CommitModel _commitModel;

        public string Node { get; private set; }

        public string User { get; private set; }

        public string Repository { get; private set; }

        public bool ShowRepository { get; private set; }

        public CommitModel Changeset
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

        public ICommand GoToRepositoryCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = User, Repository = Repository })); }
        }

        public ICommand GoToHtmlUrlCommand
        {
            get { return new MvxCommand(() => ShowViewModel<WebBrowserViewModel>(new WebBrowserViewModel.NavObject { Url = _commitModel.Url }), () => _commitModel != null); }
        }

        public CollectionViewModel<CommentModel> Comments
        {
            get { return _comments; }
        }

        public ReactiveUI.ReactiveCommand<Unit, bool> GoToOwner { get; }

        public ChangesetViewModel(IApplicationService application, IFeaturesService featuresService)
        {
            _applicationService = application;
            _featuresService = featuresService;

            GoToOwner = ReactiveUI.ReactiveCommand.Create(
                () => ShowViewModel<UserViewModel>(new UserViewModel.NavObject { Username = Changeset?.Author?.Login }),
                this.Bind(x => x.Changeset, true).Select(x => x?.Author?.Login != null));
        }

        public void Init(NavObject navObject)
        {
            User = navObject.Username;
            Repository = navObject.Repository;
            Node = navObject.Node;
            ShowRepository = navObject.ShowRepository;
            Title = "Commit " + (Node.Length > 6 ? Node.Substring(0, 6) : Node);
        }

        protected override Task Load()
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
            {
                var request = _applicationService.Client.Users[User].Repositories[Repository].Get();
                _applicationService.Client.ExecuteAsync(request)
                    .ToBackground(x => ShouldShowPro = x.Data.Private && !_featuresService.IsProEnabled);
            }

            var t1 = this.RequestModel(_applicationService.Client.Users[User].Repositories[Repository].Commits[Node].Get(), response => Changeset = response.Data);
            Comments.SimpleCollectionLoad(_applicationService.Client.Users[User].Repositories[Repository].Commits[Node].Comments.GetAll()).ToBackground();
            return t1;
        }

        public async Task AddComment(string text)
        {
            var c = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[User].Repositories[Repository].Commits[Node].Comments.Create(text));
            Comments.Items.Add(c.Data);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public string Node { get; set; }
            public bool ShowRepository { get; set; }
        }
    }
}

