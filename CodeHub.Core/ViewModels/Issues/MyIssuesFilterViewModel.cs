using System;
using ReactiveUI;
using CodeHub.Core.Factories;
using CodeHub.Core.Filters;
using System.Reactive;
using System.Linq;
using Humanizer;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Issues
{
    public class MyIssuesFilterViewModel : BaseViewModel
    {
        public IReactiveCommand<MyIssuesFilterModel> SaveCommand { get; }

        public IReactiveCommand<object> DismissCommand { get; }

        public IReactiveCommand<Unit> SelectFilterTypeCommand { get; }

        public IReactiveCommand<Unit> SelectStateCommand { get; }

        public IReactiveCommand<Unit> SelectSortCommand { get; }

        private string _labels;
        public string Labels
        {
            get { return _labels; }
            set { this.RaiseAndSetIfChanged(ref _labels, value); }
        }

        private bool _ascending;
        public bool Ascending
        {
            get { return _ascending; }
            set { this.RaiseAndSetIfChanged(ref _ascending, value); }
        }

        private IssueFilterState _filterType;
        public IssueFilterState FilterType
        {
            get { return _filterType; }
            set { this.RaiseAndSetIfChanged(ref _filterType, value); }
        }

        private IssueSort _sortType;
        public IssueSort SortType
        {
            get { return _sortType; }
            set { this.RaiseAndSetIfChanged(ref _sortType, value); }
        }

        private IssueState _state;
        public IssueState State
        {
            get { return _state; }
            set { this.RaiseAndSetIfChanged(ref _state, value); }
        }

        public MyIssuesFilterViewModel(IActionMenuFactory actionMenu)
        {
            Title = "Filter";
            State = IssueState.Open;
            SortType = IssueSort.None;
            FilterType = IssueFilterState.All;

            SaveCommand = ReactiveCommand.CreateAsyncTask(_ => Task.FromResult(new MyIssuesFilterModel(FilterType, State, SortType, Labels, Ascending)));
            SaveCommand.Subscribe(_ => DismissCommand.ExecuteIfCan());
            DismissCommand = ReactiveCommand.Create().WithSubscription(_ => Dismiss());

            SelectStateCommand = ReactiveCommand.CreateAsyncTask(async sender => {
                var options = Enum.GetValues(typeof(IssueState)).Cast<IssueState>().ToList();
                var picker = actionMenu.CreatePicker();
                foreach (var option in options)
                    picker.Options.Add(option.Humanize());
                picker.SelectedOption = options.IndexOf(State);
                var ret = await picker.Show(sender);
                State = options[ret];
            });

            SelectSortCommand = ReactiveCommand.CreateAsyncTask(async sender => {
                var options = Enum.GetValues(typeof(IssueSort)).Cast<IssueSort>().ToList();
                var picker = actionMenu.CreatePicker();
                foreach (var option in options)
                    picker.Options.Add(option.Humanize());
                picker.SelectedOption = options.IndexOf(SortType);
                var ret = await picker.Show(sender);
                SortType = options[ret];
            });

            SelectFilterTypeCommand = ReactiveCommand.CreateAsyncTask(async sender => {
                var options = Enum.GetValues(typeof(IssueFilterState)).Cast<IssueFilterState>().ToList();
                var picker = actionMenu.CreatePicker();
                foreach (var option in options)
                    picker.Options.Add(option.Humanize());
                picker.SelectedOption = options.IndexOf(FilterType);
                var ret = await picker.Show(sender);
                FilterType = options[ret];
            });
        }

        public MyIssuesFilterViewModel Init(MyIssuesFilterModel model)
        {
            Ascending = model.Ascending;
            FilterType = model.FilterType;
            Labels = model.Labels;
            State = model.Open;
            SortType = model.SortType;
            return this;
        }
    }
}

