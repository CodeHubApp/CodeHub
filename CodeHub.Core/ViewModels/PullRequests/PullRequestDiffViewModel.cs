using System;
using ReactiveUI;
using Octokit;
using System.Reactive;
using CodeHub.Core.Factories;
using System.Reactive.Linq;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestDiffViewModel : BaseViewModel, IFileDiffViewModel
    {
        private readonly ISubject<PullRequestReviewComment> _commentCreatedObservable = new Subject<PullRequestReviewComment>();

        public IObservable<PullRequestReviewComment> CommentCreated
        {
            get { return _commentCreatedObservable.AsObservable(); }
        }

        private readonly ObservableAsPropertyHelper<string> _filename;
        public string Filename
        {
            get { return _filename.Value; }
        }

        private readonly ObservableAsPropertyHelper<string> _patch;
        public string Patch
        {
            get { return _patch.Value; }
        }

        public IReadOnlyReactiveList<FileDiffCommentViewModel> Comments { get; private set; }

        private PullRequestFile _pullRequestFile;
        public PullRequestFile PullRequestFile
        {
            get { return _pullRequestFile; }
            private set { this.RaiseAndSetIfChanged(ref _pullRequestFile, value); }
        }

        private PullRequestFilesViewModel _parentViewModel;
        private PullRequestFilesViewModel ParentViewModel
        {
            get { return _parentViewModel; }
            set { this.RaiseAndSetIfChanged(ref _parentViewModel, value); }
        }

        private int? _selectedPatchLine;
        public int? SelectedPatchLine
        {
            get { return _selectedPatchLine; }
            set { this.RaiseAndSetIfChanged(ref _selectedPatchLine, value); }
        }

        public IReactiveCommand<Unit> GoToCommentCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public PullRequestDiffViewModel(ISessionService sessionService, IActionMenuFactory actionMenuFactory, IAlertDialogFactory alertDialogFactory)
        {
            var gotoCreateCommentCommand = ReactiveCommand.Create().WithSubscription(_ => {
                var vm = new ComposerViewModel(async s => {
                    var req = new PullRequestReviewCommentCreate(s, ParentViewModel.HeadSha, Filename, SelectedPatchLine ?? 0);
                    var comment = await sessionService.GitHubClient.PullRequest.Comment.Create(ParentViewModel.RepositoryOwner, ParentViewModel.RepositoryName, ParentViewModel.PullRequestId, req);
                    _commentCreatedObservable.OnNext(comment);
                }, alertDialogFactory);
                NavigateTo(vm);
            });

            GoToCommentCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedPatchLine).Select(x => x != null),
                sender => {
                    var sheet = actionMenuFactory.Create();
                    sheet.AddButton(string.Format("Add Comment on Line {0}", SelectedPatchLine), gotoCreateCommentCommand);
                    return sheet.Show(sender);
                });

            this.WhenAnyValue(x => x.PullRequestFile.Patch)
                .IsNotNull()
                .ToProperty(this, x => x.Patch, out _patch);

            var comments = new ReactiveList<PullRequestReviewComment>();
            Comments = comments.CreateDerivedCollection(
                x => new FileDiffCommentViewModel(x.User.Login, x.User.AvatarUrl, x.Body, x.Position ?? 0));

            this.WhenAnyValue(x => x.ParentViewModel.Comments)
                .Merge(this.WhenAnyObservable(x => x.ParentViewModel.Comments.Changed).Select(_ => ParentViewModel.Comments))
                .Select(x => x.Where(y => string.Equals(y.Path, Filename, StringComparison.OrdinalIgnoreCase)).ToList())
                .Subscribe(x => comments.Reset(x));

            this.WhenAnyValue(x => x.PullRequestFile.FileName)
                .ToProperty(this, x => x.Filename, out _filename);

            this.WhenAnyValue(x => x.Filename)
                .Subscribe(x => {
                    if (string.IsNullOrEmpty(x))
                        Title = "Diff";
                    else
                        Title = Path.GetFileName(Filename) ?? Filename.Substring(Filename.LastIndexOf('/') + 1);
                });
        }

        public PullRequestDiffViewModel Init(PullRequestFilesViewModel parentViewModel, PullRequestFile pullRequestFile)
        {
            PullRequestFile = pullRequestFile;
            ParentViewModel = parentViewModel;
            return this;
        }
    }
}

