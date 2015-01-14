using System;
using CodeHub.Core.Services;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelsViewModel : BaseViewModel, ILoadableViewModel
    {
        public IReadOnlyReactiveList<IssueLabelItemViewModel> Labels { get; private set; }

        public ReactiveList<Octokit.Label> SelectedLabels { get; private set; }

        public ICollection<Octokit.Label> OriginalLabels { get; set; } 

        public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

		public long IssueId { get; set; }

		public bool SaveOnSelect { get; set; }

        public IReactiveCommand SelectLabelsCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IssueLabelsViewModel(IApplicationService applicationService, IGraphicService graphicService)
	    {
            Title = "Labels";

            var labels = new ReactiveList<Octokit.Label>();
            SelectedLabels = new ReactiveList<Octokit.Label>();

            Labels = labels.CreateDerivedCollection(x => 
            {
                var vm = new IssueLabelItemViewModel(graphicService, x);
                vm.SelectCommand.Subscribe(_ =>
                {
                    var selected = SelectedLabels.FirstOrDefault(y => string.Equals(y.Name, x.Name));
                    if (selected != null)
                    {
                        SelectedLabels.Remove(selected);
                        vm.Selected = false;
                    }
                    else
                    {
                        SelectedLabels.Add(x);
                        vm.Selected = true;
                    }
                });
                return vm;
            });

            SelectLabelsCommand = ReactiveCommand.CreateAsyncTask(async t =>
	        {
	            //If nothing has changed, dont do anything...
                if (OriginalLabels != null && OriginalLabels.Count() == SelectedLabels.Count() &&
                    OriginalLabels.Intersect(SelectedLabels).Count() == SelectedLabels.Count())
	            {
                    Dismiss();
	                return;
	            }

	            if (SaveOnSelect)
	            {
//	                try
//	                {
//                        var labels = (SelectedLabels != null && SelectedLabels.Count > 0) 
//                                    ? SelectedLabels.Select(y => y.Name).ToArray() : null;
//	                    var updateReq =
//	                        applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[IssueId]
//	                            .UpdateLabels(labels);
//                        await applicationService.Client.ExecuteAsync(updateReq);
//	                }
//	                catch (Exception e)
//	                {
//	                    throw new Exception("Unable to save labels! Please try again.", e);
//	                }
	            }

                Dismiss();
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                labels.Reset(await applicationService.GitHubClient.Issue.Labels.GetForRepository(RepositoryOwner, RepositoryName)));
	    }
    }
}

