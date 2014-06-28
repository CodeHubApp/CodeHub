using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;
        private GistModel _gist;
        private bool? _starred;

        public string Id { get; set; }

        public GistModel Gist
        {
            get { return _gist; }
            set { this.RaiseAndSetIfChanged(ref _gist, value); }
        }

        public bool? IsStarred
        {
            get { return _starred; }
            private set { this.RaiseAndSetIfChanged(ref _starred, value); }
        }

		public ReactiveCollection<GistCommentModel> Comments { get; private set; }

        public IReactiveCommand GoToUserCommand { get; private set; }

        public IReactiveCommand GoToFileSourceCommand { get; private set; }

        public IReactiveCommand GoToViewableFileCommand { get; private set; }

        public IReactiveCommand GoToHtmlUrlCommand { get; private set; }

        public IReactiveCommand ForkCommand { get; private set; }

        public IReactiveCommand ToggleStarCommand { get; private set; }

		public IReactiveCommand ShareCommand { get; private set; }

        public GistViewModel(IApplicationService applicationService, IShareService shareService)
        {
            _applicationService = applicationService;
            Comments = new ReactiveCollection<GistCommentModel>();

            ShareCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Gist, x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(Gist.HtmlUrl));

            ToggleStarCommand = new ReactiveCommand(this.WhenAnyValue(x => x.IsStarred, x => x.HasValue));
            ToggleStarCommand.RegisterAsyncTask(async t =>
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

            ForkCommand = new ReactiveCommand();
            ForkCommand.RegisterAsyncTask(async t =>
            {
                var data =
                    await _applicationService.Client.ExecuteAsync(_applicationService.Client.Gists[Id].ForkGist());
                var forkedGist = data.Data;
                var vm = CreateViewModel<GistViewModel>();
                vm.Id = forkedGist.Id;
                vm.Gist = forkedGist;
                ShowViewModel(vm);
            });

            GoToViewableFileCommand = new ReactiveCommand();
            GoToViewableFileCommand.OfType<GistFileModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<GistViewableFileViewModel>();
                vm.GistFile = x;
                ShowViewModel(vm);
            });

            GoToHtmlUrlCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Gist, x => x != null && !string.IsNullOrEmpty(x.HtmlUrl)));
            GoToHtmlUrlCommand.Subscribe(_ => GoToUrlCommand.ExecuteIfCan(Gist.HtmlUrl));

            GoToFileSourceCommand = new ReactiveCommand();
            GoToFileSourceCommand.OfType<GistFileModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<GistFileViewModel>();
                vm.Id = Id;
                vm.GistFile = x;
                vm.Filename = x.Filename;
                ShowViewModel(vm);
            });

            GoToUserCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Gist, x => x != null));
            GoToUserCommand.Subscribe(x =>
            {
                var vm = CreateViewModel<ProfileViewModel>();
                vm.Username = Gist.Owner.Login;
                ShowViewModel(vm);
            });

            LoadCommand.RegisterAsyncTask(t =>
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

