using ReactiveUI;

namespace CodeHub.Core.ViewModels
{
    public interface ICanGoToUrl
    {
        IReactiveCommand GoToUrlCommand { get; }
    }
}

