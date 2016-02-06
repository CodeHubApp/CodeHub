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

		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				RaisePropertyChanged(() => Title);
			}
		}

		public string Content
		{
			get { return _content; }
			set
			{
				_content = value;
				RaisePropertyChanged(() => Content);
			}
		}

		public MilestoneModel Milestone
		{
			get { return _milestone; }
			set
			{
				_milestone = value;
				RaisePropertyChanged(() => Milestone);
			}
		}

		public CollectionViewModel<LabelModel> Labels
		{
			get { return _labels; }
		}

		public BasicUserModel AssignedTo
		{
			get { return _assignedTo; }
			set
			{
				_assignedTo = value;
				RaisePropertyChanged(() => AssignedTo);
			}
		}

		public bool IsSaving
		{
			get
			{
				return _isSaving;
			}
			set
			{
				_isSaving = value;
				RaisePropertyChanged(() => IsSaving);
			}
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

