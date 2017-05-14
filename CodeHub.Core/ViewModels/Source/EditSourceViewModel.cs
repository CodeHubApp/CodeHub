using System;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class EditSourceViewModel : LoadableViewModel
    {
        private readonly IMessageService _messageService;

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

        public EditSourceViewModel(IMessageService messageService = null)
        {
            _messageService = messageService ?? GetService<IMessageService>();
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Path = navObject.Path ?? string.Empty;
            Branch = navObject.Branch ?? "master";

            if (!Path.StartsWith("/", StringComparison.Ordinal))
                Path = "/" + Path;
        }

        protected override async Task Load()
        {
            var request = this.GetApplication().Client.Users[Username].Repositories[Repository].GetContentFile(Path, Branch);
            var data = await this.GetApplication().Client.ExecuteAsync(request);
            BlobSha = data.Data.Sha;
            Text = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data.Data.Content));
        }

        public async Task Commit(string data, string message)
        {
            var request = this.GetApplication().Client.Users[Username].Repositories[Repository].UpdateContentFile(Path, message, data, BlobSha, Branch);
            var response = await this.GetApplication().Client.ExecuteAsync(request);
            _messageService.Send(new SourceEditMessage { OldSha = BlobSha, Data = data, Update = response.Data });
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public string Path { get; set; }
            public string Branch { get; set; }
        }
    }
}

