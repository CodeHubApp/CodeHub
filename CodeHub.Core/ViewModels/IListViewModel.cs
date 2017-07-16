using System;
using System.Reactive;
using ReactiveUI;

namespace CodeHub.Core.ViewModels
{
    public interface IListViewModel<T>
    {
        ReactiveCommand<Unit, bool> LoadCommand { get; }

        ReactiveCommand<Unit, bool> LoadMoreCommand { get; }

        IReadOnlyReactiveList<T> Items { get; }

        bool HasMore { get; }

        string SearchText { get; }
    }
}
