//using System;
//using Octokit;
//using System.Reactive.Linq;
//using System.Reactive.Threading.Tasks;
//using System.Reactive.Subjects;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using System.Linq.Expressions;
//
//namespace CodeHub.Core.Repositories
//{
//    public interface IIssueRepository
//    {
//        int Id { get; }
//        string RepositoryOwner { get; }
//        string RepositoryName { get; }
//
//        IObservable<Issue> UpdateIssue(IssueUpdate update);
//        IObservable<Issue> GetIssue();
//        IObservable<IReadOnlyList<Label>> GetLabels();
//        IObservable<IReadOnlyList<User>> GetAssignees();
//        IObservable<IReadOnlyList<Milestone>> GetMilestones();
//    }
//
//    public class IssueRepository : IIssueRepository
//    {
//        private readonly IGitHubClient _client;
//        private readonly ModelItem<Issue> _cachedIssue = new ModelItem<Issue>();
//        private readonly ModelItem<IReadOnlyList<Label>> _cachedLabels = new ModelItem<IReadOnlyList<Label>>();
//        private readonly ModelItem<IReadOnlyList<Milestone>> _cachedMilestones = new ModelItem<IReadOnlyList<Milestone>>();
//        private readonly ModelItem<IReadOnlyList<User>> _cachedAssignees = new ModelItem<IReadOnlyList<User>>();
//
//        public int Id { get; }
//        public string RepositoryOwner { get; }
//        public string RepositoryName { get; }
//
//        public IssueRepository(IGitHubClient client, string repositoryOwner, string repositoryName, int id)
//        {
//            _client = client;
//            RepositoryOwner = repositoryOwner;
//            RepositoryName = repositoryName;
//            Id = id;
//        }
//
//        public IObservable<Issue> UpdateIssue(IssueUpdate update)
//            => _cachedIssue.Update(() => _client.Issue.Update(RepositoryOwner, RepositoryName, Id, update));
//
//        public IObservable<Issue> GetIssue()
//            => _cachedIssue.Get(() => _client.Issue.Get(RepositoryOwner, RepositoryName, Id));
//
//        public IObservable<IReadOnlyList<Label>> GetLabels()
//            => _cachedLabels.Get(() => _client.Issue.Labels.GetAllForRepository(RepositoryOwner, RepositoryName));
//
//        public IObservable<IReadOnlyList<User>> GetAssignees()
//            => _cachedAssignees.Get(() => _client.Issue.Assignee.GetAllForRepository(RepositoryOwner, RepositoryName));
//
//        public IObservable<IReadOnlyList<Milestone>> GetMilestones()
//            => _cachedMilestones.Get(() => _client.Issue.Milestone.GetAllForRepository(RepositoryOwner, RepositoryName));
//    }
//
//    public class ModelItem<T> where T : class
//    {
//        private T _item;
//
//        public void Update(T item) => _item = item;
//
//        public IObservable<T> Update(Func<Task<T>> func)
//        {
//            var subject = new AsyncSubject<T>();
//            subject.Subscribe(x => _item = x);
//            func().ToObservable().Subscribe(subject);
//            return subject;
//        }
//
//        public IObservable<T> Get(Func<Task<T>> func)
//        {
//            return Observable.Create<T>(new Func<IObserver<T>, Task>(async x =>
//            {
//                if (_item == null)
//                    _item = await func();
//                x.OnNext(_item);
//                x.OnCompleted();
//            }));
//        }
//    }
//}
//
