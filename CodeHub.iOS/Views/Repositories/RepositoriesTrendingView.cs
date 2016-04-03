using System;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using System.Linq;
using CoreGraphics;
using CodeHub.Core.Utilities;
using CodeHub.iOS.ViewControllers.Repositories;
using CodeHub.iOS.Transitions;
using GitHubSharp.Models;
using System.Collections.Generic;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesTrendingView : ViewModelDrivenDialogViewController
    {
        private readonly TrendingTitleButton _trendingTitleButton = new TrendingTitleButton { Frame = new CGRect(0, 0, 200f, 32f) };

        public RepositoriesTrendingView() : base(true, UITableViewStyle.Plain)
        {
            EnableSearch = false;
            NavigationItem.TitleView = _trendingTitleButton;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var vm = (RepositoriesTrendingViewModel)ViewModel;
            var weakVm = new WeakReference<RepositoriesTrendingViewModel>(vm);

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 64f;
            TableView.SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);

            vm.Bind(x => x.Repositories).Subscribe(repos => {
                var repositories = repos ?? Enumerable.Empty<Tuple<string, IList<RepositoryModel>>>();
                Root.Reset(repositories.Select(x => {
                    var s = new Section(CreateHeaderView(x.Item1));
                    s.Reset(x.Item2.Select(repo => {
                        var description = vm.ShowRepositoryDescription ? repo.Description : string.Empty;
                        var avatar = new GitHubAvatar(repo.Owner?.AvatarUrl);
                        var sse = new RepositoryElement(repo.Name, repo.StargazersCount, repo.Forks, description, repo.Owner?.Login, avatar) { ShowOwner = true };
                        sse.Tapped += MakeCallback(weakVm, repo);
                        return sse;
                    }));
                    return s;
                }));
            });

            OnActivation(d => {
                d(_trendingTitleButton.GetClickedObservable().Subscribe(_ => ShowLanguages()));
                d(vm.Bind(x => x.SelectedLanguage, true).Subscribe(l => _trendingTitleButton.Text = l.Name));
            });
        }

        private static Action MakeCallback(WeakReference<RepositoriesTrendingViewModel> weakVm, RepositoryModel model)
        {
            return new Action(() => weakVm.Get()?.GoToRepositoryCommand.Execute(model));
        }


        private void ShowLanguages()
        {
            var vm = new WeakReference<RepositoriesTrendingViewModel>(ViewModel as RepositoriesTrendingViewModel);
            var view = new LanguagesViewController();
            view.SelectedLanguage = vm.Get()?.SelectedLanguage;
            view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Images.Buttons.CancelButton };
            view.NavigationItem.LeftBarButtonItem.GetClickedObservable().Subscribe(_ => DismissViewController(true, null));
            view.Language.Subscribe(x => {
                Root.Clear();
                vm.Get().Do(y => y.SelectedLanguage = x);
                DismissViewController(true, null);
            });
            var ctrlToPresent = new ThemedNavigationController(view);
            ctrlToPresent.TransitioningDelegate = new SlideDownTransition();
            PresentViewController(ctrlToPresent, true, null);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
            {
                NavigationController.NavigationBar.ShadowImage = new UIImage();
                _trendingTitleButton.TintColor = NavigationController.NavigationBar.TintColor;
            }
        }

        private static UILabel CreateHeaderView(string name)
        {
            return new UILabel(new CGRect(0, 0, 320f, 26f)) 
            {
                BackgroundColor = Theme.CurrentTheme.PrimaryColor,
                Text = name,
                Font = UIFont.BoldSystemFontOfSize(14f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center
            };
        }
    }
}

