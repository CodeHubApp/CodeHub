using System;
using System.Reactive.Linq;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class FileSourceItemViewModel
    {
        public string FilePath { get; set; }
        public bool IsBinary { get; set; }
    }

    public interface IFileSourceViewModel : IBaseViewModel
    {
        FileSourceItemViewModel SourceItem { get; }

        string Theme { get; set; }

        IReactiveCommand<object> NextItemCommand { get; }

        IReactiveCommand<object> PreviousItemCommand { get; }
    }

    public abstract class FileSourceViewModel<TFileModel> : BaseViewModel, ILoadableViewModel, IFileSourceViewModel
    {
        public string Title { get; set; }

        private TFileModel[] _items;
        public TFileModel[] Items
        {
            get { return _items; }
            set { this.RaiseAndSetIfChanged(ref _items, value); }
        }

        private FileSourceItemViewModel _source;
        public FileSourceItemViewModel SourceItem
		{
            get { return _source; }
            protected set { this.RaiseAndSetIfChanged(ref _source, value); }
		}

        private string _theme;
        public string Theme
        {
            get { return _theme; }
            set { this.RaiseAndSetIfChanged(ref _theme, value); }
        }

        private int _currentItemIndex;
        public int CurrentItemIndex
        {
            get { return _currentItemIndex; }
            set { this.RaiseAndSetIfChanged(ref _currentItemIndex, value); }
        }

        ObservableAsPropertyHelper<TFileModel> _currentItem;
        public TFileModel CurrentItem
        {
            get { return _currentItem.Value; }
        }

        public IReactiveCommand<object> NextItemCommand { get; private set; }

        public IReactiveCommand<object> PreviousItemCommand { get; private set; }

        public abstract IReactiveCommand LoadCommand { get; }

        protected FileSourceViewModel()
        {
            this.WhenAnyValue(x => x.CurrentItemIndex)
                .Where(x => Items != null && x < Items.Length)
                .Select(x => Items[x])
                .ToProperty(this, r => r.CurrentItem, out _currentItem, scheduler: System.Reactive.Concurrency.Scheduler.Immediate);
        }

        protected void SetupRx()
        {
            var hasNextItems = 
                this.WhenAnyValue(y => y.CurrentItemIndex, x => x.Items)
                    .Select(x => x.Item2 != null && x.Item2.Length > 1 && x.Item1 < (x.Item2.Length - 1));

            NextItemCommand = ReactiveCommand.Create(
                this.LoadCommand.CanExecuteObservable.CombineLatest(
                    hasNextItems, (canLoad, hasNext) => canLoad && hasNext)
                .DistinctUntilChanged()
                .StartWith(false));
            NextItemCommand.Subscribe(x => CurrentItemIndex += 1);

            var hasPreviousItems = 
                this.WhenAnyValue(y => y.CurrentItemIndex, x => x.Items)
                    .Select(x => x.Item2 != null && x.Item2.Length > 1 && x.Item1 > 0);

            PreviousItemCommand = ReactiveCommand.Create(
                this.LoadCommand.CanExecuteObservable.CombineLatest(
                    hasPreviousItems, (canLoad, hasPrevious) => canLoad && hasPrevious)
                .DistinctUntilChanged()
                .StartWith(false));
            PreviousItemCommand.Subscribe(x => CurrentItemIndex -= 1);
        }
    }
}

