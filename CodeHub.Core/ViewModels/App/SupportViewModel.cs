using System;
using Xamarin.Utilities.Core.ViewModels;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using GitHubSharp.Models;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;

namespace CodeHub.Core.ViewModels.App
{
    public class SupportViewModel : BaseViewModel, ILoadableViewModel
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

        private RepositoryModel _repository;
        public RepositoryModel Repository
        {
            get { return _repository; }
            private set { this.RaiseAndSetIfChanged(ref _repository, value); }
        }

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand GoToFeedbackCommand { get; private set; }

        public IReactiveCommand GoToSuggestFeatureCommand { get; private set; }

        public IReactiveCommand GoToReportBugCommand { get; private set; }

        public SupportViewModel(IApplicationService applicationService)
        {
            Title = "Feedback & Support";

            _lastCommit = this.WhenAnyValue(x => x.Repository).IsNotNull()
                .Select(x => x.PushedAt).ToProperty(this, x => x.LastCommit);

            GoToFeedbackCommand = ReactiveCommand.Create().WithSubscription(_ =>
                CreateAndShowViewModel<FeedbackViewModel>());

            var gotoIssue = new Action<IssueModel>(x =>
            {
                var vm = CreateViewModel<IssueViewModel>();
                vm.Issue = x;
                vm.RepositoryName = CodeHubName;
                vm.RepositoryOwner = CodeHubOwner;
                vm.Id = x.Number;
                ShowViewModel(vm);
            });

            GoToSuggestFeatureCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<FeedbackComposerViewModel>();
                vm.IsFeature = true;
                vm.CreatedIssueObservable.Subscribe(gotoIssue);
                ShowViewModel(vm);
            });

            GoToReportBugCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<FeedbackComposerViewModel>();
                vm.IsFeature = false;
                vm.CreatedIssueObservable.Subscribe(gotoIssue);
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var repoRequest = applicationService.Client.Users[CodeHubOwner].Repositories[CodeHubName];

                this.RequestModel(repoRequest.GetContributors(), true, 
                    response => Contributors = response.Data.Count).FireAndForget();

                return this.RequestModel(repoRequest.Get(), true, response => Repository = response.Data);
            });
        }

    }
}

