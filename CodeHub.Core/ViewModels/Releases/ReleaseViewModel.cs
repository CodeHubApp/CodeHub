using System;
using CodeHub.Core.Services;
using ReactiveUI;
using GitHubSharp.Models;
using System.Reactive.Linq;
using System.Reactive;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Services;
using Xamarin.Utilities.Factories;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleaseViewModel : BaseViewModel, ILoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public long ReleaseId { get; set; }

        private readonly ObservableAsPropertyHelper<string> _contentText;
        public string ContentText { get { return _contentText.Value; } }

        private ReleaseModel _releaseModel;
        public ReleaseModel ReleaseModel
        {
            get { return _releaseModel; }
            set { this.RaiseAndSetIfChanged(ref _releaseModel, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToGitHubCommand { get; private set; }

        public IReactiveCommand<object> GoToLinkCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public ReleaseViewModel(IApplicationService applicationService, IShareService shareService, 
            IUrlRouterService urlRouterService, IActionMenuFactory actionMenuService)
        {
            this.WhenAnyValue(x => x.ReleaseModel)
                .Select(x => 
                {
                    if (x == null) return "Release";
                    var name = string.IsNullOrEmpty(x.Name) ? x.TagName : x.Name;
                    return name ?? "Release";
                })
                .Subscribe(x => Title = x);

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(new Uri(ReleaseModel.HtmlUrl)));

            var gotoUrlCommand = new Action<string>(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;
                NavigateTo(vm);
            });

            GoToGitHubCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null));
            GoToGitHubCommand.Select(_ => ReleaseModel.HtmlUrl).Subscribe(gotoUrlCommand);

            GoToLinkCommand = ReactiveCommand.Create();
            GoToLinkCommand.OfType<string>().Subscribe(x =>
            {
                var handledViewModel = urlRouterService.Handle(x);
                if (handledViewModel != null)
                    NavigateTo(handledViewModel);
                else
                    gotoUrlCommand(x);
            });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null),
                _ =>
                {
                    var menu = actionMenuService.Create(Title);
                    menu.AddButton("Share", ShowMenuCommand);
                    menu.AddButton("Show in GitHub", GoToGitHubCommand);
                    return menu.Show();
                });

            _contentText = this.WhenAnyValue(x => x.ReleaseModel).IsNotNull()
                .Select(x => x.BodyHtml).ToProperty(this, x => x.ContentText);

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => 
                this.RequestModel(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetRelease(ReleaseId), 
                    x as bool?, r => ReleaseModel = r.Data));
        }
    }
}

