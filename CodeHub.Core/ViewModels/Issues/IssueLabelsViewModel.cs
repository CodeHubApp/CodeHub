using System.Threading.Tasks;
using System.Collections.Generic;
using CodeHub.Core.Messages;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelsViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;
        private readonly IApplicationService _applicationService;
        private IEnumerable<Octokit.Label> _originalLables;

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set {
                _isSaving = value;
                RaisePropertyChanged(() => IsSaving);
            }
        }

        private readonly CollectionViewModel<Octokit.Label> _labels = new CollectionViewModel<Octokit.Label>();
        public CollectionViewModel<Octokit.Label> Labels
        {
            get { return _labels; }
        }

        private readonly CollectionViewModel<Octokit.Label> _selectedLabels = new CollectionViewModel<Octokit.Label>();
        public CollectionViewModel<Octokit.Label> SelectedLabels
        {
            get { return _selectedLabels; }
        }

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public int Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;
            SaveOnSelect = navObject.SaveOnSelect;

            _originalLables = GetService<IViewModelTxService>().Get() as IEnumerable<Octokit.Label>;
            SelectedLabels.Items.Reset(_originalLables);
        }

        public ICommand SaveLabelChoices
        {
            get { return new MvxCommand(() => SelectLabels(SelectedLabels)); }
        }

        public IssueLabelsViewModel(IMessageService messageService, IApplicationService applicationService)
        {
            _messageService = messageService;
            _applicationService = applicationService;
        }

        private async Task SelectLabels(IEnumerable<Octokit.Label> x)
        {
            //If nothing has changed, dont do anything...
            if (_originalLables != null && _originalLables.Count() == x.Count() && _originalLables.Intersect(x).Count() == x.Count())
            {
                ChangePresentation(new MvxClosePresentationHint(this));
                return;
            }
                
            if (SaveOnSelect)
            {
                try
                {
                    IsSaving = true;

                    var issueUpdate = new Octokit.IssueUpdate();
                    foreach (var label in x.Select(y => y.Name))
                        issueUpdate.AddLabel(label);

                    var newIssue = await _applicationService.GitHubClient.Issue.Update(Username, Repository, Id, issueUpdate);
                    _messageService.Send(new IssueEditMessage(newIssue));
                }
                catch
                {
                    DisplayAlert("Unable to save labels! Please try again.");
                }
                finally
                {
                    IsSaving = false;
                }
            }
            else
            {
                _messageService.Send(new SelectIssueLabelsMessage(SelectedLabels.Items));
            }

            ChangePresentation(new MvxClosePresentationHint(this));
        }

        protected override async Task Load()
        {
            var labels = await _applicationService.GitHubClient.Issue.Labels.GetAllForRepository(Username, Repository);
            Labels.Items.Reset(labels);
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

