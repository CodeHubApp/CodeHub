using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewModel : BaseViewModel, ILoadableViewModel, ICanGoToUrl
    {
        private readonly IApplicationService _applicationService;

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

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToUserCommand { get; private set; }

        public IReactiveCommand<object> GoToFileSourceCommand { get; private set; }

        public IReactiveCommand<object> GoToViewableFileCommand { get; private set; }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; private set; }

        public IReactiveCommand GoToUrlCommand { get; private set; }

        public IReactiveCommand ForkCommand { get; private set; }

        public IReactiveCommand ToggleStarCommand { get; private set; }

        public IReactiveCommand AddCommentCommand { get; private set; }

		public IReactiveCommand<object> ShareCommand { get; private set; }

        public GistViewModel(IApplicationService applicationService, IShareService shareService)
        {
            _applicationService = applicationService;
            Comments = new ReactiveList<GistCommentModel>();

            Title = "Gist";

            GoToUrlCommand = this.CreateUrlCommand();

            this.WhenAnyValue(x => x.Gist).Where(x => x != null && x.Files != null && x.Files.Count > 0)
                .Select(x => x.Files.First().Key).Subscribe(x => 
                    Title = x);

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(Gist.HtmlUrl));

            ToggleStarCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsStarred).Select(x => x.HasValue),
                async t =>
            {
                try
                {
                    if (!IsStarred.HasValue) return;
                    var request = IsStarred.Value ? _applicationService.Client.Gists[Id].Unstar() : _applicationService.Client.Gists[Id].Star();
                    await _applicationService.Client.ExecuteAsync(request);
                    IsStarred = !IsStarred.Value;
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to start gist. Please try again.", e);
                }
            });

            ForkCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                var data = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Gists[Id].ForkGist());
                var forkedGist = data.Data;
                var vm = CreateViewModel<GistViewModel>();
                vm.Id = forkedGist.Id;
                vm.Gist = forkedGist;
                ShowViewModel(vm);
            });

            GoToViewableFileCommand = ReactiveCommand.Create();
            GoToViewableFileCommand.OfType<GistFileModel>().Subscribe(x =>
            {

            });

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)));
            GoToHtmlUrlCommand.Select(_ => Gist.HtmlUrl).Subscribe(x =>
            {
                var vm = CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;

            });

            GoToFileSourceCommand = ReactiveCommand.Create();
            GoToFileSourceCommand.OfType<GistFileModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<GistFileViewModel>();
                vm.Id = Id;
                vm.GistFile = x;
                vm.Filename = x.Filename;
                ShowViewModel(vm);
            });

            GoToUserCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null && x.Owner != null));
            GoToUserCommand.Subscribe(x =>
            {
                var vm = CreateViewModel<UserViewModel>();
                vm.Username = Gist.Owner.Login;
                ShowViewModel(vm);
            });

            AddCommentCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<GistCommentViewModel>();
                vm.Id = Id;
                vm.CommentAdded.Subscribe(Comments.Add);
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var forceCacheInvalidation = t as bool?;
                var t1 = this.RequestModel(_applicationService.Client.Gists[Id].Get(), forceCacheInvalidation, response => Gist = response.Data);
			    this.RequestModel(_applicationService.Client.Gists[Id].IsGistStarred(), forceCacheInvalidation, response => IsStarred = response.Data).FireAndForget();
			    Comments.SimpleCollectionLoad(_applicationService.Client.Gists[Id].GetComments(), forceCacheInvalidation).FireAndForget();
                return t1;
            });
        }

        public async Task Edit(GistEditModel editModel)
        {
			var response = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Gists[Id].EditGist(editModel));
            Gist = response.Data;
        }
    }
}

