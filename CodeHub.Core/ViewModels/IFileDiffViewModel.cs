using ReactiveUI;
using System.Reactive;
using System;

namespace CodeHub.Core.ViewModels
{
    public interface IFileDiffViewModel : IBaseViewModel
    {
        string Patch { get; }

        Tuple<int, int> SelectedPatchLine { get; set; }

        IReadOnlyReactiveList<FileDiffCommentViewModel> Comments { get; }

        IReactiveCommand<Unit> GoToCommentCommand { get; }
    }
}

