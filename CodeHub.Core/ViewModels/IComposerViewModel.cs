using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels
{
    public interface IComposerViewModel : IBaseViewModel
    {
        string Text { get; set; }

        IReactiveCommand<Unit> SaveCommand { get; }

        IReactiveCommand<bool> DismissCommand { get; }
    }
}

