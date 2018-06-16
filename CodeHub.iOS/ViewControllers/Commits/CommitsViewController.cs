using System;
using CodeHub.Core.Services;
using CodeHub.iOS.DialogElements;
using Octokit;
using Splat;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.iOS.ViewControllers.Repositories;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Commits
{
    public static class CommitsViewController
    {
        public static CommitsViewController<GitHubCommit> RepositoryCommits(string username, string repository, string sha)
        {
            var request = new CommitRequest { Sha = sha };
            var api = ApiUrls.RepositoryCommits(username, repository);
            var uri = api.ApplyParameters(request.ToParametersDictionary());
            return new CommitsViewController<GitHubCommit>(username, repository, uri, (x, vc) =>
            {
                return new CommitElement(
                    x.Commit.Committer.Name,
                    x.GenerateCommiterAvatarUrl(),
                    x.Commit.Message,
                    x.Commit.Committer.Date,
                    () => vc.PushViewController(new CommitViewController()));
            });
        }

        public static CommitsViewController<PullRequestCommit> PullRequestCommits(string username, string repository, int pullRequestId)
        {
            var uri = ApiUrls.PullRequestCommits(username, repository, pullRequestId);
            return new CommitsViewController<PullRequestCommit>(username, repository, uri, (x, vc) =>
            {
                return new CommitElement(
                    x.Commit.Committer.Name,
                    x.GenerateCommiterAvatarUrl(),
                    x.Commit.Message,
                    x.Commit.Committer.Date,
                    () => vc.PushViewController(new CommitViewController()));
            });
        }
    }
    
    public sealed class CommitsViewController<T> : GitHubListViewController<T>
    {
        private readonly string _username, _repository;
        private readonly IFeaturesService _featuresService;
        private readonly Func<T, CommitsViewController<T>, Element> _convertToElement;

        private ReactiveCommand<Unit, bool> CheckIfPrivateRepo { get; }

        public CommitsViewController(
            string username,
            string repository,
            Uri uri,
            Func<T, CommitsViewController<T>, Element> convertToElement,
            IApplicationService applicationService = null,
            IFeaturesService featuresService = null)
            : base(uri, applicationService)
        {
            _username = username;
            _repository = repository;
            _convertToElement = convertToElement;
            _featuresService = featuresService ?? Locator.Current.GetService<IFeaturesService>();
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            CheckIfPrivateRepo = ReactiveCommand.CreateFromTask(async () =>
            {
                var repo = await applicationService.GitHubClient.Repository.Get(_username, _repository);
                return repo.Private;
            });

            CheckIfPrivateRepo
                .ThrownExceptions
                .Subscribe(err => this.Log().ErrorException("Failed to retrieve repository to check if private!", err));

            CheckIfPrivateRepo
                .Where(x => x && !_featuresService.IsProEnabled)
                .Subscribe(_ => this.ShowPrivateView());

            if (!_featuresService.IsProEnabled)
            {
                Appearing
                    .Take(1)
                    .Select(_ => Unit.Default)
                    .InvokeReactiveCommand(CheckIfPrivateRepo);
            }

            Title = "Commits";
        }

        protected override Element ConvertToElement(T item) => _convertToElement(item, this);
    }
}

