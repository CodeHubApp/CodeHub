using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using System.Reactive.Subjects;
using System;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitMessageViewModel : BaseViewModel
    {
        private readonly Subject<ContentUpdateModel> _contentSubject = new Subject<ContentUpdateModel>();

        private string _message;
        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }

        public IObservable<ContentUpdateModel> ContentChanged
        {
            get { return _contentSubject; }
        }

        public string Username { get; set; }

        public string Repository { get; set; }

        public string Path { get; set; }

        public string BlobSha { get; set; }

        public string Branch { get; set; }

        public string Text { get; set; }

        public IReactiveCommand SaveCommand { get; private set; }

        public CommitMessageViewModel(IApplicationService applicationService)
        {
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Message).Select(x => !string.IsNullOrEmpty(x)),
                async _ =>
                {
                    var request = applicationService.Client.Users[Username].Repositories[Repository]
                        .UpdateContentFile(Path, Message, Text, BlobSha, Branch);
                    var response = await applicationService.Client.ExecuteAsync(request);
                    _contentSubject.OnNext(response.Data);
                    DismissCommand.ExecuteIfCan();
                });
        }
    }
}