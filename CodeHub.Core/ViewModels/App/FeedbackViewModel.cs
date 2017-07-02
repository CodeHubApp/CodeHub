using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive;
using Splat;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackViewModel : ReactiveObject, IProvidesSearchKeyword
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

        public ReactiveCommand<Unit, bool> LoadCommand { get; private set; }

        public FeedbackViewModel(IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            var items = new ReactiveList<Octokit.Issue>();
            Items = items.CreateDerivedCollection(
                x => new FeedbackItemViewModel(x),
                x => x.Title.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateFromTask(async _ =>
            {
                var issues = await applicationService.GitHubClient.Issue.GetAllForRepository(CodeHubOwner, CodeHubName);
                items.Reset(issues);
                return issues.Count > 0;
            });
        }
    }
}

