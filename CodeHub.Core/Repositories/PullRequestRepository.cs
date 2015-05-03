using System;
using CodeHub.Core.Services;
using Octokit;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace CodeHub.Core.Repositories
{
    public class PullRequestRepository
    {
        private readonly ISessionService _sessionService;
        private PullRequest _cachedPullRequest;

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public int Id { get; private set; }

        public PullRequestRepository(ISessionService sessionService, string repositoryOwner, string repositoryName, int id)
        {
            _sessionService = sessionService;
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            Id = id;
        }
    
        public IObservable<PullRequest> GetPullRequest()
        {
            return Observable.Create<PullRequest>(x => {
                if (_cachedPullRequest != null)
                    x.OnNext(_cachedPullRequest);

                return _sessionService.GitHubClient.Repository.PullRequest.Get(RepositoryOwner, RepositoryName, Id).ToObservable()
                    .Subscribe(y => {
                        _cachedPullRequest = y;
                        x.OnNext(y);
                    }, x.OnError, x.OnCompleted);
            });
        }
//
//        public IObservable<PullRequestReviewComment> GetReviewComments()
//        {
//            
//        }
    }
}

