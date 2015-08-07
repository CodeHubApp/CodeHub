using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Contents
{
    public class CreateFileViewModel : BaseViewModel
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public string Path { get; private set; }

        public string Branch { get; private set; }

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

        public IReactiveCommand<Octokit.RepositoryContentChangeSet> SaveCommand { get; }

        public IReactiveCommand<bool> DismissCommand { get; }

        public CreateFileViewModel(ISessionService sessionService, IAlertDialogFactory alertDialogFactory)
        {
            Title = "Create File";

            this.WhenAnyValue(x => x.Name)
                .Subscribe(x => CommitMessage = "Created " + x);

            _canCommit = this.WhenAnyValue(x => x.Name)
                .Select(x => !string.IsNullOrEmpty(x))
                .ToProperty(this, x => x.CanCommit);

            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Name).Select(x => !string.IsNullOrEmpty(x)), async _ => {
                    var content = Content ?? string.Empty;
                    var path = System.IO.Path.Combine(Path ?? string.Empty, Name);
                    var request = new Octokit.CreateFileRequest(CommitMessage, content) { Branch = Branch };
                    using (alertDialogFactory.Activate("Commiting..."))
                        return await sessionService.GitHubClient.Repository.Content.CreateFile(RepositoryOwner, RepositoryName, path, request);
                });
            SaveCommand.Subscribe(x => Dismiss());

            DismissCommand = ReactiveCommand.CreateAsyncTask(async t => {
                if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Content)) return true;
                return await alertDialogFactory.PromptYesNo("Discard File?", "Are you sure you want to discard this file?");
            });
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
        }

        public CreateFileViewModel Init(string repositoryOwner, string repositoryName, string path, string branch)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            Path = path;
            Branch = branch;
            return this;
        }
    }
}

