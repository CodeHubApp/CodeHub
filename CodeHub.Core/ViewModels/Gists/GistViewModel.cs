using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Factories;
using Octokit;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewModel : BaseViewModel, ILoadableViewModel
    {
        public string Id { get; private set; }

        private Gist _gist;
        public Gist Gist
        {
            get { return _gist; }
            private set { this.RaiseAndSetIfChanged(ref _gist, value); }
        }

        private bool? _starred;
        public bool? IsStarred
        {
            get { return _starred; }
            private set { this.RaiseAndSetIfChanged(ref _starred, value); }
        }

        private readonly ObservableAsPropertyHelper<GitHubAvatar> _avatar;
        public GitHubAvatar Avatar
        {
            get { return _avatar.Value; }
        }

		public ReactiveList<GistCommentModel> Comments { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToOwnerCommand { get; private set; }

        public IReactiveCommand<object> GoToFileSourceCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public IReactiveCommand<Unit> ForkCommand { get; private set; }

        public IReactiveCommand<Unit> ToggleStarCommand { get; private set; }

        public IReactiveCommand AddCommentCommand { get; private set; }

		public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public IReactiveCommand<object> GoToUrlCommand { get; private set; }

        public GistViewModel(
            ISessionService sessionService, 
            IActionMenuFactory actionMenuService, 
            IAlertDialogFactory alertDialogFactory) 
        {
            Comments = new ReactiveList<GistCommentModel>();

            Title = "Gist";

            this.WhenAnyValue(x => x.Gist).Where(x => x != null && x.Files != null && x.Files.Count > 0)
                .Select(x => x.Files.First().Key).Subscribe(x => 
                    Title = x);

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null));
            ShareCommand.Subscribe(sender => actionMenuService.ShareUrl(sender, new Uri(Gist.HtmlUrl)));

            this.WhenAnyValue(x => x.Gist.Owner.AvatarUrl)
                .Select(x => new GitHubAvatar(x))
                .ToProperty(this, x => x.Avatar, out _avatar);

            ToggleStarCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsStarred).Select(x => x.HasValue),
                async t =>
            {
                try
                {
                    if (!IsStarred.HasValue) return;
                    var request = IsStarred.Value ? sessionService.Client.Gists[Id].Unstar() : sessionService.Client.Gists[Id].Star();
                    await sessionService.Client.ExecuteAsync(request);
                    IsStarred = !IsStarred.Value;
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to start gist. Please try again.", e);
                }
            });

            ForkCommand = ReactiveCommand.CreateAsyncTask(async t => {
                var gist = await sessionService.GitHubClient.Gist.Fork(Id);
                var vm = this.CreateViewModel<GistViewModel>();
                vm.Id = gist.Id;
                vm.Gist = gist;
                NavigateTo(vm);
            });

            ForkCommand.IsExecuting.Subscribe(x =>
            {
                if (x)
                    alertDialogFactory.Show("Forking...");
                else
                    alertDialogFactory.Hide();
            });

            GoToEditCommand = ReactiveCommand.Create();
            GoToEditCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<GistEditViewModel>();
                vm.Gist = Gist;
                vm.SaveCommand.Subscribe(x => Gist = x);
                NavigateTo(vm);
            });

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)));
            GoToHtmlUrlCommand
                .Select(_ => this.CreateViewModel<WebBrowserViewModel>())
                .Select(x => x.Init(Gist.HtmlUrl))
                .Subscribe(NavigateTo);

            GoToFileSourceCommand = ReactiveCommand.Create();
            GoToFileSourceCommand.OfType<GistFile>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<GistFileViewModel>();
                vm.Id = Id;
                vm.GistFile = x;
                vm.Filename = x.Filename;
                NavigateTo(vm);
            });

            GoToOwnerCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null && x.Owner != null));
            GoToOwnerCommand
                .Select(_ => this.CreateViewModel<UserViewModel>())
                .Select(x => x.Init(Gist.Owner.Login))
                .Subscribe(NavigateTo);

            AddCommentCommand = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(new ComposerViewModel("Add Comment", async x => {
                    var request = sessionService.Client.Gists[Id].CreateGistComment(x);
                    Comments.Add((await sessionService.Client.ExecuteAsync(request)).Data);
                }, alertDialogFactory)));

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Gist).Select(x => x != null), 
                sender => {
                    var menu = actionMenuService.Create();
                    if (Gist.Owner != null && string.Equals(sessionService.Account.Username, Gist.Owner.Login, StringComparison.OrdinalIgnoreCase))
                        menu.AddButton("Edit", GoToEditCommand);
                    else
                        menu.AddButton("Fork", ForkCommand);
                    menu.AddButton("Share", ShareCommand);
                    menu.AddButton("Show in GitHub", GoToHtmlUrlCommand);
                    return menu.Show(sender);
                });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                sessionService.GitHubClient.Gist.IsStarred(Id).ToBackground(x => IsStarred = x);
			    Comments.SimpleCollectionLoad(sessionService.Client.Gists[Id].GetComments());
                Gist = await sessionService.GitHubClient.Gist.Get(Id);
            });
        }

        public GistViewModel Init(string id, Gist gist = null)
        {
            Id = id;
            Gist = gist;
            return this;
        }
    }
}

