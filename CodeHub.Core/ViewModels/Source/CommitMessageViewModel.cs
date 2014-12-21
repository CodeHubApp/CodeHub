using Xamarin.Utilities.ViewModels;
using ReactiveUI;
using System.Reactive.Subjects;
using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using CodeHub.Core.Services;
using Xamarin.Utilities.Services;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitMessageViewModel : BaseViewModel
    {
        private readonly ISubject<ContentUpdateModel> _contentUpdateSubject = new Subject<ContentUpdateModel>();

        private string _commitMessage;
        public string CommitMessage
        {
            get { return _commitMessage; }
            set { this.RaiseAndSetIfChanged(ref _commitMessage, value); }
        }

        public IObservable<ContentUpdateModel> Saved
        {
            get { return _contentUpdateSubject; }
        }

        public string Text { get; set; }

        public string Username { get; set; }

        public string Repository { get; set; }

        public string Path { get; set; }

        public string BlobSha { get; set; }

        public string Branch { get; set; }

        public IReactiveCommand<Unit> SaveCommand { get; private set; }

        public CommitMessageViewModel(IApplicationService applicationService, IStatusIndicatorService statusIndicator)
        {
            Title = "Commit Message";

            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CommitMessage).Select(x => !string.IsNullOrEmpty(x)),
                async _ =>
            {
                var path = Path.StartsWith("/", StringComparison.Ordinal) ? Path : string.Concat("/", Path);
                var request = applicationService.Client.Users[Username].Repositories[Repository]
                    .UpdateContentFile(path, CommitMessage, Text, BlobSha, Branch);

                using (statusIndicator.Activate("Commiting..."))
                {
                    var response = await applicationService.Client.ExecuteAsync(request);
                    _contentUpdateSubject.OnNext(response.Data);
                }
            });
        }
    }
}

