using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Services;
using System.Reactive.Subjects;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.IO;
using System.Threading.Tasks;
using CodeHub.Core.Factories;
using Humanizer;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackComposerViewModel : BaseViewModel
    {
        private const string CodeHubOwner = "thedillonb";
        private const string CodeHubName = "TestTestTest";
        private readonly ISubject<Octokit.Issue> _createdIssueSubject = new Subject<Octokit.Issue>();

        public IObservable<Octokit.Issue> CreatedIssueObservable
        {
            get { return _createdIssueSubject; }
        }

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

        private bool _isFeature;
        public bool IsFeature
        {
            get { return _isFeature; }
            set { this.RaiseAndSetIfChanged(ref _isFeature, value); }
        }

        public IReactiveCommand<Unit> SubmitCommand { get; private set; }

        public IReactiveCommand<bool> DismissCommand { get; private set; }

        public FeedbackComposerViewModel(ISessionService applicationService, IAlertDialogFactory alertDialogFactory)
        {
            this.WhenAnyValue(x => x.IsFeature).Subscribe(x => Title = x ? "New Feature" : "Bug Report");

            SubmitCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Subject).Select(x => !string.IsNullOrEmpty(x)),
                async _ =>
            {
                if (string.IsNullOrEmpty(Subject))
                    throw new ArgumentException(string.Format("You must provide a title for this {0}!", IsFeature ? "feature" : "bug"));

                var labels = await applicationService.GitHubClient.Issue.Labels.GetForRepository(CodeHubOwner, CodeHubName);
                var createLabels = labels.Where(x => string.Equals(x.Name, IsFeature ? "feature request" : "bug", StringComparison.OrdinalIgnoreCase)).Select(x => x.Name).Distinct();

                var createIssueRequest = new Octokit.NewIssue(Subject) { Body = Description };
                foreach (var label in createLabels)
                    createIssueRequest.Labels.Add(label);
                var createdIssue = await applicationService.GitHubClient.Issue.Create(CodeHubOwner, CodeHubName, createIssueRequest);

                _createdIssueSubject.OnNext(createdIssue);
                Dismiss();
            });

            DismissCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                if (string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(Subject))
                    return true;
                var itemType = IsFeature ? "feature" : "bug";
                return await alertDialogFactory.PromptYesNo("Discard " + itemType.Transform(To.TitleCase) + "?", "Are you sure you want to discard this " + itemType + "?");
            });
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
        }
    }
}

