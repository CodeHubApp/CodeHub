using System;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels
{
    public abstract class BaseSearchableListViewModel<TItem, TViewModel> : BaseListViewModel<TItem, TViewModel>, IProvidesSearchKeyword
    {
        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }
    }

    public interface IListViewModel<TViewModel>
    {
        IReadOnlyReactiveList<TViewModel> Items { get; }
    }

    public abstract class BaseListViewModel<TItem, TViewModel> : BaseViewModel, IListViewModel<TViewModel>, IPaginatableViewModel, IProvidesEmpty
    {
        protected readonly ReactiveList<TItem> InternalItems = new ReactiveList<TItem>(resetChangeThreshold: double.MaxValue);

        public IReadOnlyReactiveList<TViewModel> Items { get; protected set; }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            protected set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        private IReactiveCommand<Unit> _loadCommand;
        public IReactiveCommand<Unit> LoadCommand
        {
            get { return _loadCommand; }
            protected set { this.RaiseAndSetIfChanged(ref _loadCommand, value); }
        }

        private bool _isEmpty;
        public bool IsEmpty 
        { 
            get { return _isEmpty; } 
            private set { this.RaiseAndSetIfChanged(ref _isEmpty, value); }
        }

        protected BaseListViewModel()
        {
            this.WhenAnyValue(x => x.LoadCommand).IsNotNull().Switch().Take(1)
                .Select(_ => InternalItems.CountChanged.StartWith(InternalItems.Count).Select(x => x == 0))
                .Switch().Subscribe(x => IsEmpty = x);
        }
    }
}

