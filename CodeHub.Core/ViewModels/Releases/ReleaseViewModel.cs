using System;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.Services;
using ReactiveUI;
using GitHubSharp.Models;
using Xamarin.Utilities.Core.Services;
using System.Reactive.Linq;
using System.Reactive;

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

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToGitHubCommand { get; private set; }

        public IReactiveCommand<object> GoToLinkCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public ReleaseViewModel(IApplicationService applicationService, IShareService shareService, 
            IUrlRouterService urlRouterService, IActionMenuService actionMenuService)
        {
            this.WhenAnyValue(x => x.ReleaseModel)
                .Select(x => x == null ? "Release" : x.Name).Subscribe(x => Title = x);

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(ReleaseModel.HtmlUrl));

            var gotoUrlCommand = this.CreateUrlCommand();

            GoToGitHubCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ReleaseModel).Select(x => x != null));
            GoToGitHubCommand.Select(_ => ReleaseModel.HtmlUrl).Subscribe(gotoUrlCommand.ExecuteIfCan);

            GoToLinkCommand = ReactiveCommand.Create();
            GoToLinkCommand.OfType<string>().Subscribe(x =>
            {
                var handledViewModel = urlRouterService.Handle(x);
                if (handledViewModel != null && applicationService.Account.OpenUrlsInApp)
                    ShowViewModel(handledViewModel);
                else
                    gotoUrlCommand.ExecuteIfCan(x);
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

