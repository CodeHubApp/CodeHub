using CodeHub.Core.Services;
using System;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : BaseViewModel, ILoadableViewModel
    {
        private Octokit.Milestone _selectedMilestone;
        public Octokit.Milestone SelectedMilestone
        {
            get { return _selectedMilestone; }
            set { this.RaiseAndSetIfChanged(ref _selectedMilestone, value); }
        }

        public IReadOnlyReactiveList<Octokit.Milestone> Milestones { get; private set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public long IssueId { get; set; }

        public bool SaveOnSelect { get; set; }

        public IReactiveCommand SelectMilestoneCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IssueMilestonesViewModel(IApplicationService applicationService)
        {
            Title = "Milestones";

            var milestones = new ReactiveList<Octokit.Milestone>();
            Milestones = milestones.CreateDerivedCollection(x => x);

            SelectMilestoneCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                var milestone = t as Octokit.Milestone;
                if (milestone != null)
                    SelectedMilestone = milestone;

                if (SaveOnSelect)
                {
                    try
                    {
                        int? milestoneNumber = null;
                        if (SelectedMilestone != null) milestoneNumber = SelectedMilestone.Number;
                        var updateReq = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[IssueId].UpdateMilestone(milestoneNumber);
                        await applicationService.Client.ExecuteAsync(updateReq);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to to save milestone! Please try again.", e);
                    }
                }

                Dismiss();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                milestones.Reset(await applicationService.GitHubClient.Issue.Milestone.GetForRepository(RepositoryOwner, RepositoryName)));
        }
    }
}

