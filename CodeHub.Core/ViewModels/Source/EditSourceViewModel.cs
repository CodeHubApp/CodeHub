using System;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Source
{
    public class EditSourceViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;
        private readonly IApplicationService _applicationService;

        private string _text;
        public string Text
        {
            get { return _text; }
            private set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public string Path { get; private set; }

        public string BlobSha { get; private set; }

        public string Branch { get; private set; }

        public EditSourceViewModel(
            string username,
            string repository,
            string path,
            string branch,
            IApplicationService applicationService = null,
            IMessageService messageService = null)
        {
            _applicationService = applicationService ?? GetService<IApplicationService>();
            _messageService = messageService ?? GetService<IMessageService>();

            Username = username;
            Repository = repository;
            Path = path ?? string.Empty;
            Branch = branch ?? "master";

            if (!Path.StartsWith("/", StringComparison.Ordinal))
                Path = "/" + Path;
        }

        protected override async Task Load()
        {
            var result = await _applicationService.GitHubClient.Repository.Content.GetAllContentsByRef(
                Username, Repository, Path, Branch);

            if (result.Count == 0)
                throw new Exception("Path contains no files!");

            var data = result[0];
            BlobSha = data.Sha;
            Text = data.Content;
        }

        public async Task Commit(string data, string message)
        {
            var updateRequest = new Octokit.UpdateFileRequest(message, data, BlobSha, Branch);
            var result = await _applicationService.GitHubClient.Repository.Content.UpdateFile(
                Username, Repository, Path, updateRequest);

            _messageService.Send(new SourceEditMessage {
                OldSha = BlobSha,
                Data = data,
                Update = result
            });
        }
    }
}

