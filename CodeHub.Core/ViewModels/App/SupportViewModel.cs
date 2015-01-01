using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Utilities.ViewModels;
using System.Reactive;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.Core.ViewModels.App
{
    public class SupportViewModel : BaseViewModel
    {
        private const string CodeHubOwner = "thedillonb";
        private const string CodeHubName = "codehub";

        private int? _contributors;
        public int? Contributors
        {
            get { return _contributors; }
            private set { this.RaiseAndSetIfChanged(ref _contributors, value); }
        }

        private readonly ObservableAsPropertyHelper<DateTimeOffset?> _lastCommit;
        public DateTimeOffset? LastCommit
        {
            get { return _lastCommit.Value; }
        }

        private Octokit.Repository _repository;
        public Octokit.Repository Repository
        {
            get { return _repository; }
            private set { this.RaiseAndSetIfChanged(ref _repository, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand GoToFeedbackCommand { get; private set; }

        public IReactiveCommand GoToSuggestFeatureCommand { get; private set; }

        public IReactiveCommand GoToReportBugCommand { get; private set; }

        public IReactiveCommand GoToRepositoryCommand { get; private set; }

        public SupportViewModel(IApplicationService applicationService)
        {
            Title = "Feedback & Support";

            _lastCommit = this.WhenAnyValue(x => x.Repository).IsNotNull()
                .Select(x => x.PushedAt).ToProperty(this, x => x.LastCommit);

            GoToFeedbackCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<FeedbackViewModel>()));

            var gotoIssue = new Action<Octokit.Issue>(x =>
            {
                var vm = this.CreateViewModel<IssueViewModel>();
                vm.Issue = x;
                vm.RepositoryName = CodeHubName;
                vm.RepositoryOwner = CodeHubOwner;
                vm.Id = x.Number;
                NavigateTo(vm);
            });

            GoToSuggestFeatureCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<FeedbackComposerViewModel>();
                vm.IsFeature = true;
                vm.CreatedIssueObservable.Subscribe(gotoIssue);
                NavigateTo(vm);
            });

            GoToReportBugCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<FeedbackComposerViewModel>();
                vm.IsFeature = false;
                vm.CreatedIssueObservable.Subscribe(gotoIssue);
                NavigateTo(vm);
            });

            GoToRepositoryCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = CodeHubOwner;
                vm.RepositoryName = CodeHubName;
                NavigateTo(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                applicationService.GitHubClient.Repository.GetAllContributors(CodeHubOwner, CodeHubName)
                    .ContinueWith(x => Contributors = x.Result.Count, new CancellationToken(),
                        TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
                Repository = await applicationService.GitHubClient.Repository.Get(CodeHubOwner, CodeHubName);
            });
        }

    }
}

