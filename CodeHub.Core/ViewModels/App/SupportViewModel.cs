using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using System.Reactive;
using Splat;

namespace CodeHub.Core.ViewModels.App
{
    public class SupportViewModel : ReactiveObject, ILoadableViewModel
    {
        public readonly static string CodeHubOwner = "codehubapp";
        public readonly static string CodeHubName = "codehub";

        private int? _contributors;
        public int? Contributors
        {
            get { return _contributors; }
            private set { this.RaiseAndSetIfChanged(ref _contributors, value); }
        }

        public string Title => "Feedback & Support";

        private readonly ObservableAsPropertyHelper<DateTimeOffset?> _lastCommit;
        public DateTimeOffset? LastCommit => _lastCommit.Value;

        private Octokit.Repository _repository;
        public Octokit.Repository Repository
        {
            get { return _repository; }
            private set { this.RaiseAndSetIfChanged(ref _repository, value); }
        }

        public ReactiveCommand<Unit, Unit> LoadCommand { get; }

        public SupportViewModel(IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            _lastCommit = this
                .WhenAnyValue(x => x.Repository).Where(x => x != null)
                .Select(x => x.PushedAt).ToProperty(this, x => x.LastCommit);

            LoadCommand = ReactiveCommand.CreateFromTask(async _ =>
            {
                applicationService.GitHubClient.Repository.GetAllContributors(CodeHubOwner, CodeHubName)
                        .ToBackground(x => Contributors = x.Count);
                Repository = await applicationService.GitHubClient.Repository.Get(CodeHubOwner, CodeHubName);
            });
        }

    }
}

