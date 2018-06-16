using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using Splat;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelsViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;
        private List<Octokit.Label> _originalLables;

        public ReactiveList<Octokit.Label> SelectedLabels { get; } = new ReactiveList<Octokit.Label>();

        public ReactiveList<Octokit.Label> Labels { get; } = new ReactiveList<Octokit.Label>();

        public string Username  { get; }

        public string Repository { get; }

        public int Id { get; }

        public bool SaveOnSelect { get; }

        public ReactiveCommand<Unit, Unit> SelectLabelsCommand { get; }

        public IssueLabelsViewModel(
            string username,
            string repository,
            int id,
            IEnumerable<Octokit.Label> originalLabels = null,
            bool saveOnSelect = false,
            IMessageService messageService = null)
        {
            Username = username;
            Repository = repository;
            Id = id;
            SaveOnSelect = saveOnSelect;
            _messageService = messageService ?? Locator.Current.GetService<IMessageService>();

            _originalLables = (originalLabels ?? Enumerable.Empty<Octokit.Label>()).ToList();
            SelectedLabels.AddRange(_originalLables);

            SelectLabelsCommand = ReactiveCommand.CreateFromTask(SelectLabels);

            SelectLabelsCommand
                .ThrownExceptions
                .Select(err => new UserError("Unable to assign labels! Please try again."))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();
        }

        private async Task SelectLabels()
        {
            //If nothing has changed, dont do anything...
            if (_originalLables.Count == SelectedLabels.Count 
                && _originalLables.Intersect(SelectedLabels).Count() == SelectedLabels.Count)
            {
                return;
            }
                
            if (SaveOnSelect)
            {
                var update = new Octokit.IssueUpdate();

                foreach (var label in SelectedLabels)
                    update.AddLabel(label.Name);

                var result = await this.GetApplication().GitHubClient.Issue.Update(Username, Repository, Id, update);
                _messageService.Send(new IssueEditMessage(result));
            }
            else
            {
                _messageService.Send(new SelectIssueLabelsMessage(SelectedLabels));
            }
        }

        protected override async Task Load()
        {
            var result = await this.GetApplication().GitHubClient.Issue.Labels.GetAllForRepository(Username, Repository);
            Labels.Reset(result);
        }
    }
}

