using Xamarin.Utilities.ViewModels;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels
{
    public interface IPaginatableViewModel : ILoadableViewModel
    {
        IReactiveCommand<Unit> LoadMoreCommand { get; }
    }
}

