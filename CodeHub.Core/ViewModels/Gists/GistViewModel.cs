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

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewModel : BaseViewModel, ILoadableViewModel
    {
        public string Id { get; set; }

        private GistModel _gist;
        public GistModel Gist
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

        public IReactiveCommand GoToEditCommand { get; private set; }

        public IReactiveCommand<object> GoToUrlCommand { get; private set; }

        public GistViewModel(IApplicationService applicationService, IShareService shareService, 
            IActionMenuFactory actionMenuService, IStatusIndicatorService statusIndicatorService) 
        {
            Comments = new ReactiveList<GistCommentModel>();

            Title = "Gist";

            this.WhenAnyValue(x => x.Gist).Where(x => x != null && x.Files != null && x.Files.Count > 0)
                .Select(x => x.Files.First().Key).Subscribe(x => 
                    Title = x);

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(new Uri(Gist.HtmlUrl)));

            ToggleStarCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsStarred).Select(x => x.HasValue),
                async t =>
            {
                try
                {
                    if (!IsStarred.HasValue) return;
                    var request = IsStarred.Value ? applicationService.Client.Gists[Id].Unstar() : applicationService.Client.Gists[Id].Star();
                    await applicationService.Client.ExecuteAsync(request);
                    IsStarred = !IsStarred.Value;
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to start gist. Please try again.", e);
                }
            });

            ForkCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                var data = await applicationService.Client.ExecuteAsync(applicationService.Client.Gists[Id].ForkGist());
                var forkedGist = data.Data;
                var vm = this.CreateViewModel<GistViewModel>();
                vm.Id = forkedGist.Id;
                vm.Gist = forkedGist;
                NavigateTo(vm);
            });

            ForkCommand.IsExecuting.Subscribe(x =>
            {
                if (x)
                    statusIndicatorService.Show("Forking...");
                else
                    statusIndicatorService.Hide();
            });

            GoToEditCommand = ReactiveCommand.Create();

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)));
            GoToHtmlUrlCommand.Select(_ => Gist.HtmlUrl).Subscribe(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;
                NavigateTo(vm);
            });

            GoToFileSourceCommand = ReactiveCommand.Create();
            GoToFileSourceCommand.OfType<GistFileModel>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<GistFileViewModel>();
                vm.Id = Id;
                vm.GistFile = x;
                vm.Filename = x.Filename;
                NavigateTo(vm);
            });

            GoToOwnerCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null && x.Owner != null));
            GoToOwnerCommand.Subscribe(x =>
            {
                var vm = this.CreateViewModel<UserViewModel>();
                vm.Username = Gist.Owner.Login;
                NavigateTo(vm);
            });

            AddCommentCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<GistCommentViewModel>();
                vm.Id = int.Parse(Id);
                vm.SaveCommand.Subscribe(x => LoadCommand.ExecuteIfCan());
                NavigateTo(vm);
            });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Gist).Select(x => x != null), 
                _ =>
            {
                var menu = actionMenuService.Create(Title);
                if (Gist.Owner != null && string.Equals(applicationService.Account.Username, Gist.Owner.Login, StringComparison.OrdinalIgnoreCase))
                    menu.AddButton("Edit", GoToEditCommand);
                else
                    menu.AddButton("Fork", ForkCommand);
                menu.AddButton("Share", ShareCommand);
                menu.AddButton("Show in GitHub", GoToHtmlUrlCommand);
                return menu.Show();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var forceCacheInvalidation = t as bool?;
                var t1 = this.RequestModel(applicationService.Client.Gists[Id].Get(), forceCacheInvalidation, response => Gist = response.Data);
			    this.RequestModel(applicationService.Client.Gists[Id].IsGistStarred(), forceCacheInvalidation, response => IsStarred = response.Data);
			    Comments.SimpleCollectionLoad(applicationService.Client.Gists[Id].GetComments(), forceCacheInvalidation);
                return t1;
            });
        }
    }
}

