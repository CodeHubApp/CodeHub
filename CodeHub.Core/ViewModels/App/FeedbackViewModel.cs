using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive;
using Splat;
using Octokit;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackViewModel : ReactiveObject, IProvidesSearchKeyword
    {
        private const string CodeHubOwner = "codehubapp";
        private const string CodeHubName = "codehub";

        public IReadOnlyReactiveList<FeedbackItemViewModel> Items { get; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        private bool? _isEmpty;
        public bool? IsEmpty
        {
            get { return _isEmpty; }
            set { this.RaiseAndSetIfChanged(ref _isEmpty, value); }
        }

        public string Title => "Feedback";

        public ReactiveCommand<Unit, Unit> LoadCommand { get; private set; }

        public FeedbackViewModel(IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            var items = new ReactiveList<Issue>(resetChangeThreshold: 1);
            var feedbackItems = items.CreateDerivedCollection(
                x => new FeedbackItemViewModel(CodeHubOwner, CodeHubName, x));

            Items = feedbackItems.CreateDerivedCollection(
                x => x,
                x => x.Title.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateFromTask(async _ =>
            {
                items.Clear();

                var list = applicationService.GitHubClient.RetrieveList<Issue>(
                    ApiUrls.Issues(CodeHubOwner, CodeHubName));

                var issues = await list.Next();
                IsEmpty = issues?.Count > 0;

                while (issues != null)
                {
                    items.AddRange(issues);
                    issues = await list.Next();
                }
            });
        }
    }
}

