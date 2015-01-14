using System;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Services;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Contents
{
    public class CreateFileViewModel : BaseViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public string Path { get; set; }

        public string Branch { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
        }

        private string _commitMessage;
        public string CommitMessage
        {
            get { return _commitMessage; }
            set { this.RaiseAndSetIfChanged(ref _commitMessage, value); }
        }

        private readonly ObservableAsPropertyHelper<bool> _canCommit;
        public bool CanCommit
        {
            get { return _canCommit.Value; }
        }

        public IReactiveCommand<Unit> SaveCommand { get; private set; }

        public CreateFileViewModel(IApplicationService applicationService)
        {
            Path = "/";
            Branch = "master";
            Title = "Create File";

            this.WhenAnyValue(x => x.Name).Subscribe(x => CommitMessage = "Created " + x);

            _canCommit = this.WhenAnyValue(x => x.Name)
                .Select(x => !string.IsNullOrEmpty(x))
                .ToProperty(this, x => x.CanCommit);

            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Name).Select(x => !string.IsNullOrEmpty(x)), 
                async _ =>
            {
                var content = Content ?? string.Empty;

                var path = Path;
                if (string.IsNullOrEmpty(Path))
                    path = "/";
                path = System.IO.Path.Combine(path, Name);
                var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].UpdateContentFile(path, CommitMessage, content, null, Branch);
                await applicationService.Client.ExecuteAsync(request);
                Dismiss();
            });
        }
    }
}

