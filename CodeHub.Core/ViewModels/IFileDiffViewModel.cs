using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels
{
    public interface IFileDiffViewModel : IBaseViewModel
    {
        string Patch { get; }

        int? SelectedPatchLine { get; set; }

        IReadOnlyReactiveList<FileDiffCommentViewModel> Comments { get; }

        IReactiveCommand<Unit> GoToCommentCommand { get; }
    }
}

