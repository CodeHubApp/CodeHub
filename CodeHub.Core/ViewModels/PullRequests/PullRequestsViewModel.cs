using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.Messages;
using System.Linq;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;
        private IDisposable _pullRequestEditSubscription;

        private readonly CollectionViewModel<PullRequestModel> _pullrequests = new CollectionViewModel<PullRequestModel>();
        public CollectionViewModel<PullRequestModel> PullRequests
        {
            get { return _pullrequests; }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set 
            {
                _selectedFilter = value;
                RaisePropertyChanged(() => SelectedFilter);
            }
        }

        public ICommand GoToPullRequestCommand
        {
            get { return new MvxCommand<PullRequestModel>(x => ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = Username, Repository = Repository, Id = x.Number })); }
        }

        public PullRequestsViewModel(IMessageService messageService)
        {
            _messageService = messageService;
            this.Bind(x => x.SelectedFilter).Subscribe(_ => LoadCommand.Execute(null));
        }

        public void Init(NavObject navObject) 
        {
            Username = navObject.Username;
            Repository = navObject.Repository;

            PullRequests.FilteringFunction = x => {
                var state = SelectedFilter == 0 ? "open" : "closed";
                return x.Where(y => y.State == state);
            };

            _pullRequestEditSubscription = _messageService.Listen<PullRequestEditMessage>(x =>
            {
                if (x.PullRequest == null)
                    return;
          
                var index = PullRequests.Items.IndexOf(x.PullRequest);
                if (index < 0)
                    return;
                PullRequests.Items[index] = x.PullRequest;
                PullRequests.Refresh();
            });
        }

        protected override Task Load()
        {
            var state = SelectedFilter == 0 ? "open" : "closed";
            var request = this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests.GetAll(state: state);
            return PullRequests.SimpleCollectionLoad(request);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}
