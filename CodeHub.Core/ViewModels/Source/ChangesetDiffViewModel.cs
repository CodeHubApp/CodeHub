using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using Octokit;
using CodeHub.Core.Factories;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace CodeHub.Core.ViewModels.Source
{
    public class ChangesetDiffViewModel : BaseViewModel, ILoadableViewModel, IFileDiffViewModel
    {
        private string _actualFilename;
        private readonly ISubject<CommitComment> _commentCreatedObservable = new Subject<CommitComment>();

        public IObservable<CommitComment> CommentCreated
        {
            get { return _commentCreatedObservable.AsObservable(); }
        }

		public string Username { get; set; }

		public string Repository { get; set; }

		public string Branch { get; set; }

        private string _filename;
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

        private Tuple<int, int> _selectedPatchLine;
        public Tuple<int, int> SelectedPatchLine
        {
            get { return _selectedPatchLine; }
            set { this.RaiseAndSetIfChanged(ref _selectedPatchLine, value); }
        }

        public IReadOnlyReactiveList<FileDiffCommentViewModel> Comments { get; private set; }

        public IReactiveCommand<Unit> GoToCommentCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public ChangesetDiffViewModel(ISessionService sessionService, IActionMenuFactory actionMenuFactory, IAlertDialogFactory alertDialogFactory)
	    {
            var comments = new ReactiveList<CommitComment>();
            Comments = comments.CreateDerivedCollection(
                x => new FileDiffCommentViewModel(x.User.Login, x.User.AvatarUrl, x.Body, x.Position ?? 0));

            var gotoCreateCommentCommand = ReactiveCommand.Create().WithSubscription(_ => {
                var vm = new ComposerViewModel(async s => {
                    var cmd = new NewCommitComment(s) { Path = Filename, Position = SelectedPatchLine.Item1 };
                    var comment = await sessionService.GitHubClient.Repository.RepositoryComments.Create(Username, Repository, Branch, cmd);
                    _commentCreatedObservable.OnNext(comment);
                    comments.Add(comment);
                }, alertDialogFactory);
                NavigateTo(vm);
            });

            GoToCommentCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedPatchLine).Select(x => x != null),
                sender => {
                    var sheet = actionMenuFactory.Create();
                    sheet.AddButton(string.Format("Add Comment on Line {0}", SelectedPatchLine.Item2), gotoCreateCommentCommand);
                    return sheet.Show(sender);
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

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
	        {
                sessionService.GitHubClient.Repository.RepositoryComments.GetAllForCommit(Username, Repository, Branch)
                        .ToBackground(x => comments.Reset(x.Where(y => string.Equals(y.Path, Filename))));
                var commits = await sessionService.GitHubClient.Repository.Commits.Get(Username, Repository, Branch);
                CommitFile = commits.Files.FirstOrDefault(x => string.Equals(x.Filename, Filename, StringComparison.Ordinal));
	        });
	    }
    }
}

