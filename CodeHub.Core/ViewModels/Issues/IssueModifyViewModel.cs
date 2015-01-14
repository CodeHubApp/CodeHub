using System;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
	public abstract class IssueModifyViewModel : BaseViewModel
    {
        private string _subject;
        public string Subject
        {
            get { return _subject; }
            set { this.RaiseAndSetIfChanged(ref _subject, value); }
        }

		private string _content;
		public string Content
		{
			get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
		}

        private Octokit.Milestone _milestone;
        public Octokit.Milestone Milestone
		{
			get { return _milestone; }
            set { this.RaiseAndSetIfChanged(ref _milestone, value); }
		}

        private Octokit.Label[] _labels;
        public Octokit.Label[] Labels
        {
            get { return _labels; }
            set { this.RaiseAndSetIfChanged(ref _labels, value); }
        }

        private Octokit.User _assignedTo;
        public Octokit.User AssignedTo
		{
			get { return _assignedTo; }
            set { this.RaiseAndSetIfChanged(ref _assignedTo, value); }
		}

		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public IReactiveCommand<object> GoToLabelsCommand { get; private set; }

        public IReactiveCommand<object> GoToMilestonesCommand { get; private set; }

        public IReactiveCommand<object> GoToAssigneeCommand { get; private set; }

		public IReactiveCommand<Unit> SaveCommand { get; private set; }

        protected IssueModifyViewModel(IStatusIndicatorService statusIndicatorService)
	    {
            SaveCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                using (statusIndicatorService.Activate("Saving..."))
                    await Save();
                Dismiss();
            });

            GoToAssigneeCommand = ReactiveCommand.Create();
	        GoToAssigneeCommand.Subscribe(_ =>
	        {
	            var vm = this.CreateViewModel<IssueAssignedToViewModel>();
	            vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
	            vm.SelectedUser = AssignedTo;
	            vm.WhenAnyValue(x => x.SelectedUser).Subscribe(x => AssignedTo = x);
                NavigateTo(vm);
	        });


            GoToMilestonesCommand = ReactiveCommand.Create();
            GoToMilestonesCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<IssueMilestonesViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.SelectedMilestone = Milestone;
                vm.WhenAnyValue(x => x.SelectedMilestone).Subscribe(x => Milestone = x);
                NavigateTo(vm);
            });

            GoToLabelsCommand = ReactiveCommand.Create();
            GoToLabelsCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<IssueLabelsViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.SelectedLabels.Reset(Labels);
                vm.OriginalLabels = Labels;
                vm.WhenAnyValue(x => x.SelectedLabels).Subscribe(x => Labels = x.ToArray());
                NavigateTo(vm);
            });
	    }

		protected abstract Task Save();
    }
}

