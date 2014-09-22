using System;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelsViewModel : BaseViewModel, ILoadableViewModel
    {
        public IReadOnlyReactiveList<IssueLabelItemViewModel> Labels { get; private set; }

	    public ReactiveList<LabelModel> SelectedLabels { get; private set; }

        public ICollection<LabelModel> OriginalLabels { get; set; } 

        public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

		public long IssueId { get; set; }

		public bool SaveOnSelect { get; set; }

        public IReactiveCommand SelectLabelsCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public IssueLabelsViewModel(IApplicationService applicationService, IGraphicService graphicService)
	    {
            var labels = new ReactiveList<LabelModel>();
            SelectedLabels = new ReactiveList<LabelModel>();

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
	            var selectedLabels = t as IEnumerable<LabelModel>;
                if (selectedLabels != null)
	                SelectedLabels.Reset(selectedLabels);

	            //If nothing has changed, dont do anything...
                if (OriginalLabels != null && OriginalLabels.Count() == SelectedLabels.Count() &&
                    OriginalLabels.Intersect(SelectedLabels).Count() == SelectedLabels.Count())
	            {
	                DismissCommand.ExecuteIfCan();
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

                DismissCommand.ExecuteIfCan();
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                labels.LoadAll(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Labels.GetAll()));
	    }
    }
}

