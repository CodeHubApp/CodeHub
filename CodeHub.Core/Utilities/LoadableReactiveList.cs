using System;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.Utilities
{
    public class LoadableReactiveList<T> : ReactiveObject
    {
        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private bool _loaded;
        public bool Loaded
        {
            get { return _loaded; }
            set { this.RaiseAndSetIfChanged(ref _loaded, value); }
        }

        public IReadOnlyReactiveList<T> Items { get; private set; }

        public LoadableReactiveList(IReadOnlyReactiveList<T> items, IReactiveCommand<Unit> loadCommand)
        {
            Items = items;
            LoadCommand = loadCommand;
            loadCommand.Subscribe(_ => Loaded = true);
        }
    }

    public static class LoadableReactiveList
    {
        public static LoadableReactiveList<T> Create<T>(IReadOnlyReactiveList<T> items, IReactiveCommand<Unit> loadCommand)
        {
            return new LoadableReactiveList<T>(items, loadCommand);
        }
    }
}

