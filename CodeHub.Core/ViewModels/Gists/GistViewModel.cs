using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.User;
using Octokit;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewModel : LoadableViewModel
    {
        public string Id { get; }

        private Gist _gist;
        public Gist Gist
        {
            get { return _gist; }
            set { this.RaiseAndSetIfChanged(ref _gist, value); }
        }

        private bool? _starred;
        public bool? IsStarred
        {
            get { return _starred; }
            private set { this.RaiseAndSetIfChanged(ref _starred, value); }
        }

        public ReactiveList<GistComment> Comments { get; } = new ReactiveList<GistComment>();

        public ReactiveCommand<Unit, UserViewModel> GoToUserCommand { get; }

        public ReactiveCommand<Unit, string> GoToHtmlCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleStarCommand { get; }

        public ReactiveCommand<Unit, Gist> ForkCommand { get; }

        public static GistViewModel FromGist(Gist gist)
        {
            return new GistViewModel(gist.Id) { Gist = gist };
        }

        public GistViewModel(string id)
        {
            Id = id;

            ToggleStarCommand = ReactiveCommand.CreateFromTask(
                ToggleStar,
                this.WhenAnyValue(x => x.IsStarred).Select(x => x != null));

            ToggleStarCommand
                .ThrownExceptions
                .Select(err => new UserError("Unable to " + (IsStarred.GetValueOrDefault() ? "unstar" : "star") + " this gist! Please try again.", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            ForkCommand = ReactiveCommand.CreateFromTask(ForkGist);

            GoToUserCommand = ReactiveCommand.Create(
                () => new UserViewModel(Gist.Owner.Login),
                this.WhenAnyValue(x => x.Gist.Owner.Login).Select(x => x != null));

            GoToHtmlCommand = ReactiveCommand.Create(
                () => Gist.HtmlUrl,
                this.WhenAnyValue(x => x.Gist).Select(x => x != null));
        }

        private async Task ToggleStar()
        {
            if (IsStarred == null)
                return;

            var application = this.GetApplication();

            if (IsStarred.Value)
                await application.GitHubClient.Gist.Unstar(Id);
            else
                await application.GitHubClient.Gist.Star(Id);

            IsStarred = !IsStarred;
        }

        public Task<Gist> ForkGist()
        {
            return this.GetApplication().GitHubClient.Gist.Fork(Id);
        }

        protected override async Task Load()
        {
            this.GetApplication().GitHubClient.Gist.IsStarred(Id)
                .ToBackground(x => IsStarred = x);

            this.GetApplication().GitHubClient.Gist.Comment.GetAllForGist(Id)
                .ToBackground(Comments.Reset);

            Gist = await this.GetApplication().GitHubClient.Gist.Get(Id);
        }

        public async Task Edit(GistUpdate editModel)
        {
            Gist = await this.GetApplication().GitHubClient.Gist.Edit(Id, editModel);
        }
    }
}

