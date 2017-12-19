using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Services;
using System;
using System.Linq;
using System.Reactive.Linq;
using Humanizer;
using Splat;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackComposerViewModel : ReactiveObject
    {
        private const string CodeHubOwner = "codehubapp";
        private const string CodeHubName = "codehub";

        private string _subject;
        public string Subject
        {
            get { return _subject; }
            set { this.RaiseAndSetIfChanged(ref _subject, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }

        public string Title => "Open Issue";

        public ReactiveCommand<Unit, Unit> SubmitCommand { get; private set; }

        public ReactiveCommand<Unit, bool> DismissCommand { get; private set; }

        public FeedbackComposerViewModel(
            IApplicationService applicationService = null,
            IAlertDialogService alertDialogService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            alertDialogService = alertDialogService ?? Locator.Current.GetService<IAlertDialogService>();

            SubmitCommand = ReactiveCommand.CreateFromTask(async _ =>
            {
                if (string.IsNullOrEmpty(Subject))
                    throw new ArgumentException("You must provide a title for this issue!");

                var createIssueRequest = new Octokit.NewIssue(Subject) { Body = Description };

                await applicationService.GitHubClient.Issue.Create(CodeHubOwner, CodeHubName, createIssueRequest);
            }, this.WhenAnyValue(x => x.Subject).Select(x => !string.IsNullOrEmpty(x)));

            SubmitCommand
                .ThrownExceptions
                .Select(ex => new UserError("There was a problem trying to post your feedback.", ex))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            DismissCommand = ReactiveCommand.CreateFromTask(async t =>
            {
                if (string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(Subject))
                    return true;
                
                return await alertDialogService.PromptYesNo(
                    "Discard Issue?",
                    "Are you sure you want to discard this issue?");
            });
        }
    }
}

