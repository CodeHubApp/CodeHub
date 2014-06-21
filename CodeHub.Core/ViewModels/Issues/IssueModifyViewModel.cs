using System;
using System.Linq;
using GitHubSharp.Models;
using System.Threading.Tasks;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
	public abstract class IssueModifyViewModel : BaseViewModel
    {
		private string _title;
		private string _content;
		private BasicUserModel _assignedTo;
	    private LabelModel[] _labels;
		private MilestoneModel _milestone;

		public string Title
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

        public LabelModel[] Labels
        {
            get { return _labels; }
            set { this.RaiseAndSetIfChanged(ref _labels, value); }
        }

		public BasicUserModel AssignedTo
		{
			get { return _assignedTo; }
            set { this.RaiseAndSetIfChanged(ref _assignedTo, value); }
		}

		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public IReactiveCommand GoToLabelsCommand { get; private set; }

        public IReactiveCommand GoToMilestonesCommand { get; private set; }

        public IReactiveCommand GoToAssigneeCommand { get; private set; }

		public IReactiveCommand SaveCommand { get; private set; }

	    protected IssueModifyViewModel()
	    {
            SaveCommand = new ReactiveCommand();
	        SaveCommand.RegisterAsyncTask(t => Save());

            GoToAssigneeCommand = new ReactiveCommand();
	        GoToAssigneeCommand.Subscribe(_ =>
	        {
	            var vm = CreateViewModel<IssueAssignedToViewModel>();
	            vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
	            vm.SelectedUser = AssignedTo;
	            vm.WhenAnyValue(x => x.SelectedUser).Subscribe(x => AssignedTo = x);
                ShowViewModel(vm);
	        });


            GoToMilestonesCommand = new ReactiveCommand();
            GoToMilestonesCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<IssueMilestonesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.SelectedMilestone = Milestone;
                vm.WhenAnyValue(x => x.SelectedMilestone).Subscribe(x => Milestone = x);
                ShowViewModel(vm);
            });

            GoToLabelsCommand = new ReactiveCommand();
            GoToLabelsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<IssueLabelsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.SelectedLabels.Reset(Labels);
                vm.OriginalLabels = Labels;
                vm.WhenAnyValue(x => x.SelectedLabels).Subscribe(x => Labels = x.ToArray());
                ShowViewModel(vm);
            });
	    }

		protected abstract Task Save();
    }
}

