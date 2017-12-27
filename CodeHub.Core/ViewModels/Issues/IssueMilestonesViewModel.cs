using System.Threading.Tasks;
using CodeHub.Core.Messages;
using System;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;
        private readonly IApplicationService _applicationService;

        private Octokit.Milestone _selectedMilestone;
        public Octokit.Milestone SelectedMilestone
        {
            get
            {
                return _selectedMilestone;
            }
            set
            {
                _selectedMilestone = value;
                RaisePropertyChanged(() => SelectedMilestone);
            }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set {
                _isSaving = value;
                RaisePropertyChanged(() => IsSaving);
            }
        }

        private readonly CollectionViewModel<Octokit.Milestone> _milestones = new CollectionViewModel<Octokit.Milestone>();
        public CollectionViewModel<Octokit.Milestone> Milestones
        {
            get { return _milestones; }
        }

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public int Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        public IssueMilestonesViewModel(IMessageService messageService, IApplicationService applicationService)
        {
            _messageService = messageService;
            _applicationService = applicationService;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;
            SaveOnSelect = navObject.SaveOnSelect;
            SelectedMilestone = TxSevice.Get() as Octokit.Milestone;

            this.Bind(x => x.SelectedMilestone)
                .SelectMany(x => SelectMilestone(x).ToObservable())
                .Subscribe();
        }

        private async Task SelectMilestone(Octokit.Milestone x)
        {
            if (SaveOnSelect)
            {
                try
                {
                    IsSaving = true;

                    var issueUpdate = new Octokit.IssueUpdate { Milestone = x?.Number };
                    var newIssue = await _applicationService.GitHubClient.Issue.Update(Username, Repository, Id, issueUpdate);
                    _messageService.Send(new IssueEditMessage(newIssue));
                }
                catch
                {
                    DisplayAlert("Unable to to save milestone! Please try again.");
                }
                finally
                {
                    IsSaving = false;
                }
            }
            else
            {
                _messageService.Send(new SelectedMilestoneMessage(x));
            }

            ChangePresentation(new MvvmCross.Core.ViewModels.MvxClosePresentationHint(this));
        }

        protected override async Task Load()
        {
            var milestones = await _applicationService.GitHubClient.Issue.Milestone.GetAllForRepository(Username, Repository);
            Milestones.Items.Reset(milestones);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public int Id { get; set; }
            public bool SaveOnSelect { get; set; }
        }
    }
}

