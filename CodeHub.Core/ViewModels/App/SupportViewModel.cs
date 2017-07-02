using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using System.Reactive;
using CodeHub.Core.ViewModels.Repositories;
using Splat;

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

        private Octokit.Repository _repository;
        public Octokit.Repository Repository
        {
            get { return _repository; }
            private set { this.RaiseAndSetIfChanged(ref _repository, value); }
        }

        public ReactiveCommand<Unit, Unit> LoadCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> GoToFeedbackCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> GoToSuggestFeatureCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> GoToReportBugCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> GoToRepositoryCommand { get; private set; }

        public SupportViewModel(IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            Title = "Feedback & Support";

            _lastCommit = this
                .WhenAnyValue(x => x.Repository).Where(x => x != null)
                .Select(x => x.PushedAt).ToProperty(this, x => x.LastCommit);

            //GoToFeedbackCommand = ReactiveCommand.Create(() =>
            //    NavigateTo(this.CreateViewModel<FeedbackViewModel>()));

            //var gotoIssue = new Action<Octokit.Issue>(x =>
            //{
            //    var vm = this.CreateViewModel<IssueViewModel>();
            //    vm.Init(CodeHubOwner, CodeHubName, x.Number, x);
            //    NavigateTo(vm);
            //});

            //GoToSuggestFeatureCommand = ReactiveCommand.Create().WithSubscription(_ =>
            //{
            //    var vm = this.CreateViewModel<FeedbackComposerViewModel>();
            //    vm.IsFeature = true;
            //    vm.CreatedIssueObservable.Subscribe(gotoIssue);
            //    NavigateTo(vm);
            //});

            //GoToReportBugCommand = ReactiveCommand.Create().WithSubscription(_ =>
            //{
            //    var vm = this.CreateViewModel<FeedbackComposerViewModel>();
            //    vm.IsFeature = false;
            //    vm.CreatedIssueObservable.Subscribe(gotoIssue);
            //    NavigateTo(vm);
            //});

            //GoToRepositoryCommand = ReactiveCommand.Create().WithSubscription(_ =>
            //{
            //    var vm = this.CreateViewModel<RepositoryViewModel>();
            //    vm.Init(CodeHubOwner, CodeHubName);
            //    NavigateTo(vm);
            //});

            LoadCommand = ReactiveCommand.CreateFromTask(async _ =>
            {
                applicationService.GitHubClient.Repository.GetAllContributors(CodeHubOwner, CodeHubName)
                        .ToBackground(x => Contributors = x.Count);
                Repository = await applicationService.GitHubClient.Repository.Get(CodeHubOwner, CodeHubName);
            });
        }

    }
}

