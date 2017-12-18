using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels.User;
using MvvmCross.Core.ViewModels;
using Octokit;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<GistComment> _comments = new CollectionViewModel<GistComment>();
        private Gist _gist;
        private bool _starred;

        public string Id
        {
            get;
            private set;
        }

        public Gist Gist
        {
            get { return _gist; }
            set { this.RaiseAndSetIfChanged(ref _gist, value); }
        }

        public bool IsStarred
        {
            get { return _starred; }
            private set { this.RaiseAndSetIfChanged(ref _starred, value); }
        }

        public CollectionViewModel<GistComment> Comments
        {
            get { return _comments; }
        }

        public ICommand GoToUserCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserViewModel>(new UserViewModel.NavObject { Username = Gist.Owner.Login }), () => Gist != null && Gist.Owner != null); }
        }

        public ICommand GoToHtmlUrlCommand
        {
            get { return new MvxCommand(() => GoToUrlCommand.Execute(_gist.HtmlUrl), () => _gist != null); }
        }

        public ICommand ForkCommand
        {
            get
            {
                return new MvxCommand(() => ForkGist());
            }
        }

        public ICommand ToggleStarCommand
        {
            get
            {
                return new MvxCommand(() => ToggleStarred(), () => Gist != null);
            }
        }

        public static GistViewModel FromGist(Gist gist)
        {
            return new GistViewModel
            {
                Gist = gist,
                Id = gist.Id
            };
        }

        public void Init(NavObject navObject)
        {
            Id = navObject.Id;
        }

        private async Task ToggleStarred()
        {
            try
            {
                var request = IsStarred ? this.GetApplication().Client.Gists[Id].Unstar() : this.GetApplication().Client.Gists[Id].Star();
                await this.GetApplication().Client.ExecuteAsync(request);
                IsStarred = !IsStarred;
            }
            catch
            {
                DisplayAlert("Unable to start gist. Please try again.");
            }
        }

        public async Task ForkGist()
        {
            var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Gists[Id].ForkGist());
            var forkedGist = data.Data;
            ShowViewModel<GistViewModel>(new GistViewModel.NavObject { Id = forkedGist.Id });
        }

        protected override async Task Load()
        {
            Comments.Items.Clear();

            this.GetApplication().GitHubClient.Gist.IsStarred(Id)
                .ToBackground(x => IsStarred = x);

            this.GetApplication().GitHubClient.Gist.Comment.GetAllForGist(Id)
                .ToBackground(Comments.Items.AddRange);

            Gist = await this.GetApplication().GitHubClient.Gist.Get(Id);
        }

        public async Task Edit(GistUpdate editModel)
        {
            Gist = await this.GetApplication().GitHubClient.Gist.Edit(Id, editModel);
        }

        public class NavObject
        {
            public string Id { get; set; }
        }
    }
}

