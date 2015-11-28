using System;
using CodeHub.Core.Services;
using ReactiveUI;
using GitHubSharp.Models;
using System.Reactive.Linq;
using System.Reactive;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleaseViewModel : BaseViewModel, ILoadableViewModel
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public long ReleaseId { get; private set; }

        private readonly ObservableAsPropertyHelper<string> _contentText;
        public string ContentText 
        { 
            get { return _contentText.Value; } 
        }

        private ReleaseModel _releaseModel;
        public ReleaseModel ReleaseModel
        {
            get { return _releaseModel; }
            private set { this.RaiseAndSetIfChanged(ref _releaseModel, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReactiveCommand<object> GoToLinkCommand { get; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; }

        public ReleaseViewModel(ISessionService applicationService,
            IUrlRouterService urlRouterService, IActionMenuFactory actionMenuService)
        {
            Title = "Release";

            this.WhenAnyValue(x => x.ReleaseModel)
                .Select(x => 
                {
                    if (x == null) return "Release";
                    var name = string.IsNullOrEmpty(x.Name) ? x.TagName : x.Name;
                    return name ?? "Release";
                })
                .Subscribe(x => Title = x);

            var shareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null));
            shareCommand.Subscribe(sender => actionMenuService.ShareUrl(sender, ReleaseModel.HtmlUrl));

            var gotoUrlCommand = new Action<string>(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Init(x);
                NavigateTo(vm);
            });

            var gotoGitHubCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null));
            gotoGitHubCommand.Select(_ => ReleaseModel.HtmlUrl).Subscribe(gotoUrlCommand);

            GoToLinkCommand = ReactiveCommand.Create();
            GoToLinkCommand.OfType<string>().Subscribe(x =>
            {
                var handledViewModel = urlRouterService.Handle(x);
                if (handledViewModel != null)
                    NavigateTo(handledViewModel);
                else
                    gotoUrlCommand(x);
            });

            var canShowMenu = this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null);
            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(canShowMenu, sender => {
                var menu = actionMenuService.Create();
                menu.AddButton("Share", shareCommand);
                menu.AddButton("Show in GitHub", gotoGitHubCommand);
                return menu.Show(sender);
            });

            _contentText = this.WhenAnyValue(x => x.ReleaseModel).IsNotNull()
                .Select(x => x.BodyHtml).ToProperty(this, x => x.ContentText);

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetRelease(ReleaseId);
                ReleaseModel = (await applicationService.Client.ExecuteAsync(request)).Data;
            });
        }

        public ReleaseViewModel Init(string repositoryOwner, string repositoryName, long id)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            ReleaseId = id;
            return this;
        }
    }
}

