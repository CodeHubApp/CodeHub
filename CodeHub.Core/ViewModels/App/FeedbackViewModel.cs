using System;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using Xamarin.Utilities.ViewModels;
using System.Reactive;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        private const string CodeHubOwner = "thedillonb";
        private const string CodeHubName = "codehub";

        public IReadOnlyReactiveList<FeedbackItemViewModel> Items { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public FeedbackViewModel(IApplicationService applicationService)
        {
            Title = "Feedback";

            var items = new ReactiveList<Octokit.Issue>();
            Items = items.CreateDerivedCollection(x => new FeedbackItemViewModel(x, () =>
                {
                    var vm = this.CreateViewModel<IssueViewModel>();
                    vm.RepositoryOwner = CodeHubOwner;
                    vm.RepositoryName = CodeHubName;
                    vm.Id = x.Number;
                    NavigateTo(vm);
                }), 
                filter: x => x.Title.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                items.Reset(await applicationService.GitHubClient.Issue.GetForRepository(CodeHubOwner, CodeHubName)));
        }
    }
}

