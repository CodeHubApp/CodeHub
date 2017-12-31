using System.Threading.Tasks;
using GitHubSharp.Models;
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
        private IEnumerable<LabelModel> _originalLables;

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set {
                _isSaving = value;
                RaisePropertyChanged(() => IsSaving);
            }
        }

        private readonly CollectionViewModel<LabelModel> _labels = new CollectionViewModel<LabelModel>();
        public CollectionViewModel<LabelModel> Labels
        {
            get { return _labels; }
        }

        private readonly CollectionViewModel<LabelModel> _selectedLabels = new CollectionViewModel<LabelModel>();
        public CollectionViewModel<LabelModel> SelectedLabels
        {
            get { return _selectedLabels; }
        }

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public long Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Id = navObject.Id;
            SaveOnSelect = navObject.SaveOnSelect;

            _originalLables = GetService<CodeHub.Core.Services.IViewModelTxService>().Get() as IEnumerable<LabelModel>;
            SelectedLabels.Items.Reset(_originalLables);
        }

        public ICommand SaveLabelChoices
        {
            get { return new MvxCommand(() => SelectLabels(SelectedLabels)); }
        }

        public IssueLabelsViewModel(IMessageService messageService)
        {
            _messageService = messageService;
        }

        private async Task SelectLabels(IEnumerable<LabelModel> x)
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
                    var labels = x != null ? x.Select(y => y.Name).ToArray() : null;
                    var updateReq = this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].UpdateLabels(labels);
                    var newIssue = await this.GetApplication().Client.ExecuteAsync(updateReq);
                    _messageService.Send(new IssueEditMessage(newIssue.Data));
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

        protected override Task Load()
        {
            return Labels.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Labels.GetAll());
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

