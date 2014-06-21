using CodeHub.Core.Services;
using GitHubSharp.Models;
using System;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : LoadableViewModel
    {
        private MilestoneModel _selectedMilestone;
        public MilestoneModel SelectedMilestone
        {
            get { return _selectedMilestone; }
            set { this.RaiseAndSetIfChanged(ref _selectedMilestone, value); }
        }

        public ReactiveCollection<MilestoneModel> Milestones { get; private set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public long IssueId { get; set; }

        public bool SaveOnSelect { get; set; }

        public IReactiveCommand SelectMilestoneCommand { get; private set; }

        public IssueMilestonesViewModel(IApplicationService applicationService)
        {
            Milestones = new ReactiveCollection<MilestoneModel>();

            SelectMilestoneCommand = new ReactiveCommand();
            SelectMilestoneCommand.RegisterAsyncTask(async t =>
            {
                var milestone = t as MilestoneModel;
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

                DismissCommand.ExecuteIfCan();
            });

            LoadCommand.RegisterAsyncTask(t =>
                Milestones.SimpleCollectionLoad(
                    applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Milestones.GetAll(),
                    t as bool?));
        }
    }
}

