using System;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using MvvmCross.Plugins.Messenger;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using System.Threading.Tasks;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class IssueModifyViewModel : BaseViewModel
    {
        private string _title;
        private string _content;
        private BasicUserModel _assignedTo;
        private readonly CollectionViewModel<LabelModel> _labels = new CollectionViewModel<LabelModel>();
        private MilestoneModel _milestone;
        private MvxSubscriptionToken _labelsToken, _milestoneToken, _assignedToken;
        private bool _isSaving;

        public string IssueTitle
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }

        public string Content
        {
            get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
        }

        public MilestoneModel Milestone
        {
            get { return _milestone; }
            set { this.RaiseAndSetIfChanged(ref _milestone, value); }
        }

        public CollectionViewModel<LabelModel> Labels
        {
            get { return _labels; }
        }

        public BasicUserModel AssignedTo
        {
            get { return _assignedTo; }
            set { this.RaiseAndSetIfChanged(ref _assignedTo, value); }
        }

        public bool IsSaving
        {
            get { return _isSaving; }
            protected set { this.RaiseAndSetIfChanged(ref _isSaving, value); }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public ICommand GoToLabelsCommand
        {
            get 
            { 
                return new MvxCommand(() => {
                    GetService<CodeHub.Core.Services.IViewModelTxService>().Add(Labels);
                    ShowViewModel<IssueLabelsViewModel>(new IssueLabelsViewModel.NavObject { Username = Username, Repository = Repository });
                }); 
            }
        }

        public ICommand GoToMilestonesCommand
        {
            get 
            { 
                return new MvxCommand(() => {
                    GetService<CodeHub.Core.Services.IViewModelTxService>().Add(Milestone);
                    ShowViewModel<IssueMilestonesViewModel>(new IssueMilestonesViewModel.NavObject { Username = Username, Repository = Repository });
                });
            }
        }

        public ICommand GoToAssigneeCommand
        {
            get 
            { 
                return new MvxCommand(() => {
                    GetService<CodeHub.Core.Services.IViewModelTxService>().Add(AssignedTo);
                    ShowViewModel<IssueAssignedToViewModel>(new IssueAssignedToViewModel.NavObject { Username = Username, Repository = Repository });
                }); 
            }
        }

        public ICommand SaveCommand
        {
            get { return new MvxCommand(() => Save()); }
        }

        protected void Init(string username, string repository)
        {
            Username = username;
            Repository = repository;

            var messenger = GetService<IMvxMessenger>();
            _labelsToken = messenger.SubscribeOnMainThread<SelectIssueLabelsMessage>(x => Labels.Items.Reset(x.Labels));
            _milestoneToken = messenger.SubscribeOnMainThread<SelectedMilestoneMessage>(x => Milestone = x.Milestone);
            _assignedToken = messenger.SubscribeOnMainThread<SelectedAssignedToMessage>(x => AssignedTo = x.User);
        }

        protected abstract Task Save();
    }
}

