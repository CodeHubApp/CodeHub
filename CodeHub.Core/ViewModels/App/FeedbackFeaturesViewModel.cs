using System;
using Xamarin.Utilities.Core.ViewModels;
using ReactiveUI;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Issues;
using Xamarin.Utilities.Core;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackFeaturesViewModel : BaseViewModel, ILoadableViewModel
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

        public IReactiveCommand LoadCommand { get; private set; }

        public FeedbackFeaturesViewModel(IApplicationService applicationService)
        {
            Title = "Feedback";

            var items = new ReactiveList<IssueModel>();
            Items = items.CreateDerivedCollection(x => CreateItemViewModel(x), 
                x => x.Title.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var request = applicationService.Client.Users[CodeHubOwner].Repositories[CodeHubName].Issues.GetAll();
                return items.SimpleCollectionLoad(request, t as bool?);
            });
        }

        private FeedbackItemViewModel CreateItemViewModel(IssueModel x)
        {
            return new FeedbackItemViewModel(x, () =>
            {
                var vm = CreateViewModel<IssueViewModel>();
                vm.RepositoryOwner = CodeHubOwner;
                vm.RepositoryName = CodeHubName;
                vm.Id = x.Number;
                vm.Issue = x;
                ShowViewModel(vm);
            });
        }
    }
}

