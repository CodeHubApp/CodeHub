using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Source;
using MvvmCross.Platform;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class ChangesetViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<CommentModel> _comments = new CollectionViewModel<CommentModel>();
        private readonly IApplicationService _application;
        private readonly IFeaturesService _featuresService;
        private CommitModel _commitModel;

		public string Node { get; private set; }

		public string User { get; private set; }

		public string Repository { get; private set; }

        public bool ShowRepository { get; private set; }

        public CommitModel Changeset
        {
            get { return _commitModel; }
            private set
            {
                _commitModel = value;
                RaisePropertyChanged();
            }
        }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set
            {
                _shouldShowPro = value;
                RaisePropertyChanged();
            }
        }

        public ICommand GoToRepositoryCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = User, Repository = Repository })); }
        }

		public ICommand GoToFileCommand
		{
			get
			{ 
				return new MvxCommand<CommitModel.CommitFileModel>(x =>
				{
						if (x.Patch == null)
						{
							ShowViewModel<SourceViewModel>(new SourceViewModel.NavObject { GitUrl = x.ContentsUrl, HtmlUrl = x.BlobUrl, Name = x.Filename, Path = x.Filename, ForceBinary = true });
						}
						else
						{
							Mvx.Resolve<CodeHub.Core.Services.IViewModelTxService>().Add(x);
							ShowViewModel<ChangesetDiffViewModel>(new ChangesetDiffViewModel.NavObject { Username = User, Repository = Repository, Branch = _commitModel.Sha, Filename = x.Filename });
						}

				});
			}
		}

		public ICommand GoToHtmlUrlCommand
		{
			get { return new MvxCommand(() => ShowViewModel<WebBrowserViewModel>(new WebBrowserViewModel.NavObject { Url = _commitModel.Url }), () => _commitModel != null); }
		}

        public CollectionViewModel<CommentModel> Comments
        {
            get { return _comments; }
        }

        public ChangesetViewModel(IApplicationService application, IFeaturesService featuresService)
        {
            _application = application;
            _featuresService = featuresService;
        }

        public void Init(NavObject navObject)
        {
            User = navObject.Username;
            Repository = navObject.Repository;
            Node = navObject.Node;
            ShowRepository = navObject.ShowRepository;
        }

        protected override Task Load(bool forceCacheInvalidation)
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
                this.RequestModel(this.GetApplication().Client.Users[User].Repositories[Repository].Get(), false, x => ShouldShowPro = x.Data.Private && !_featuresService.IsProEnabled);

            var t1 = this.RequestModel(_application.Client.Users[User].Repositories[Repository].Commits[Node].Get(), forceCacheInvalidation, response => Changeset = response.Data);
			Comments.SimpleCollectionLoad(_application.Client.Users[User].Repositories[Repository].Commits[Node].Comments.GetAll(), forceCacheInvalidation).FireAndForget();
            return t1;
        }

        public async Task AddComment(string text)
        {
            var c = await _application.Client.ExecuteAsync(_application.Client.Users[User].Repositories[Repository].Commits[Node].Comments.Create(text));
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

