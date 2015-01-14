using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using Octokit;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Source
{
    public class ChangesetDiffViewModel : BaseViewModel, ILoadableViewModel
    {
        private string _filename;
		private string _actualFilename;

		public string Username { get; set; }

		public string Repository { get; set; }

		public string Branch { get; set; }

	    public string Filename
	    {
	        get { return _filename; }
	        set { this.RaiseAndSetIfChanged(ref _filename, value); }
	    }

        private readonly ObservableAsPropertyHelper<string> _patch;
        public string Patch
        {
            get { return _patch.Value; }
        }

        private GitHubCommitFile _commitFile;
	    public GitHubCommitFile CommitFile
	    {
	        get { return _commitFile; }
	        set { this.RaiseAndSetIfChanged(ref _commitFile, value); }
	    }

        public IReadOnlyReactiveList<CommitComment> Comments { get; private set; }

        public IReactiveCommand<object> GoToCommentCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public ChangesetDiffViewModel(IApplicationService applicationService, IActionMenuFactory actionMenuFactory)
	    {
            var comments = new ReactiveList<CommitComment>();
            Comments = comments.CreateDerivedCollection(x => x);

            GoToCommentCommand = ReactiveCommand.Create();
            GoToCommentCommand.OfType<int?>().Subscribe(line =>
            {
//                var vm = this.CreateViewModel<CommentViewModel>();
//                ReactiveUI.Legacy.ReactiveCommandMixins.RegisterAsyncTask(vm.SaveCommand, async t =>
//                {
//                    var req = applicationService.Client.Users[Username].Repositories[Repository].Commits[Branch].Comments.Create(vm.Comment, Filename, line);
//                    var response = await applicationService.Client.ExecuteAsync(req);
//			        comments.Add(response.Data);
//                    Dismiss();
//                });
//                NavigateTo(vm);
            });

            _patch = this.WhenAnyValue(x => x.CommitFile)
                .IsNotNull().Select(x => x.Patch).ToProperty(this, x => x.Patch);

	        this.WhenAnyValue(x => x.Filename).Subscribe(x =>
	        {
	            if (string.IsNullOrEmpty(x))
                    Title = "Diff";
	            else
	            {
	                _actualFilename = Path.GetFileName(Filename) ??
	                                  Filename.Substring(Filename.LastIndexOf('/') + 1);
                    Title = _actualFilename;
	            }
	        });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(_ =>
            {
                var sheet = actionMenuFactory.Create(Title);
                sheet.AddButton("Add Comment", ReactiveCommand.Create());
                return sheet.Show();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
	        {
                Observable.FromAsync(() => applicationService.GitHubClient.Repository.RepositoryComments.GetForCommit(Username, Repository, Branch))
                    .ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => comments.Reset(x));
                var commits = await applicationService.GitHubClient.Repository.Commits.Get(Username, Repository, Branch);
                CommitFile = commits.Files.FirstOrDefault(x => string.Equals(x.Filename, Filename, StringComparison.Ordinal));
	        });
	    }
    }
}

