using System;
using ReactiveUI;
using Octokit;
using System.Reactive;
using CodeHub.Core.Factories;
using System.Reactive.Linq;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestDiffViewModel : BaseViewModel
    {
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

        private ObservableAsPropertyHelper<IReadOnlyList<PullRequestReviewComment>> _comments;
        public IReadOnlyList<PullRequestReviewComment> Comments
        {
            get { return _comments.Value; }
        }

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

        public IReactiveCommand<object> GoToCommentCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public PullRequestDiffViewModel(IActionMenuFactory actionMenuFactory)
        {
            GoToCommentCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedPatchLine).Select(x => x != null));
            GoToCommentCommand.Subscribe(_ => {
                var vm = this.CreateViewModel<PullRequestCommentViewModel>();
                vm.Init(_parentViewModel.RepositoryOwner, _parentViewModel.RepositoryName, _parentViewModel.PullRequestId,
                    _pullRequestFile.Sha, _pullRequestFile.FileName, SelectedPatchLine.Value);
                NavigateTo(vm);
            });

            this.WhenAnyValue(x => x.PullRequestFile.Patch)
                .IsNotNull()
                .ToProperty(this, x => x.Patch, out _patch);

            this.WhenAnyValue(x => x.ParentViewModel.Comments)
                .Select(_ => ParentViewModel.Comments)
                .ToProperty(this, x => x.Comments, out _comments);

            this.WhenAnyValue(x => x.PullRequestFile.FileName)
                .ToProperty(this, x => x.Filename, out _filename);

            this.WhenAnyValue(x => x.Filename)
                .Subscribe(x => {
                    if (string.IsNullOrEmpty(x))
                        Title = "Diff";
                    else
                        Title = Path.GetFileName(Filename) ?? Filename.Substring(Filename.LastIndexOf('/') + 1);
                });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(sender => {
                var sheet = actionMenuFactory.Create();
                sheet.AddButton("Add Diff Comment", ReactiveCommand.Create());
                return sheet.Show(sender);
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

