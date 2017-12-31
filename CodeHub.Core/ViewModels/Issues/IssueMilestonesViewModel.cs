using System.Threading.Tasks;
using GitHubSharp.Models;
using CodeHub.Core.Messages;
using System;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;

        private MilestoneModel _selectedMilestone;
        public MilestoneModel SelectedMilestone
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

        private readonly CollectionViewModel<MilestoneModel> _milestones = new CollectionViewModel<MilestoneModel>();
        public CollectionViewModel<MilestoneModel> Milestones
        {
            get { return _milestones; }
        }

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public long Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        public IssueMilestonesViewModel(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;
            SaveOnSelect = navObject.SaveOnSelect;
            SelectedMilestone = TxSevice.Get() as MilestoneModel;

            this.Bind(x => x.SelectedMilestone).Subscribe(x => SelectMilestone(x));
        }

        private async Task SelectMilestone(MilestoneModel x)
        {
            if (SaveOnSelect)
            {
                try
                {
                    IsSaving = true;
                    int? milestone = null;
                    if (x != null) milestone = x.Number;
                    var updateReq = this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].UpdateMilestone(milestone);
                    var newIssue = await this.GetApplication().Client.ExecuteAsync(updateReq);
                    _messageService.Send(new IssueEditMessage(newIssue.Data));
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

        protected override Task Load()
        {
            return Milestones.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Milestones.GetAll());
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public long Id { get; set; }
            public bool SaveOnSelect { get; set; }
        }
    }
}

