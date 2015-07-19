using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.iOS.TableViewSources;
using CoreGraphics;
using System.Collections.Generic;
using RepositoryStumble.Transitions;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoriesTrendingViewController : BaseTableViewController<RepositoriesTrendingViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Pulse.ToEmptyListImage(), "There are no trending repositories."));

            var titleButton = new TrendingTitleButton
            {
                Frame = new CGRect(0, 0, 200f, 32f),
                TintColor = Theme.PrimaryNavigationBarTextColor
            };
            NavigationItem.TitleView = titleButton;
            titleButton.TouchUpInside += (sender, e) => ViewModel.GoToLanguages.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.SelectedLanguage).IsNotNull()
                .Subscribe(x => titleButton.Text = x.Name);

            Appearing
                .Where(_ => NavigationController != null)
                .Subscribe(_ => NavigationController.NavigationBar.ShadowImage = new UIImage());

            Disappearing
                .Where(_ => NavigationController != null)
                .Subscribe(_ => NavigationController.NavigationBar.ShadowImage = null);

            var source = new RepositoryTableViewSource(TableView);
            TableView.Source = source;

            this.WhenAnyValue(x => x.ViewModel.Repositories)
                .Select(x => x ?? new List<GroupedCollection<RepositoryItemViewModel>>())
                .Select(x => x.Select(g => new TableSectionInformation<RepositoryItemViewModel, RepositoryCellView>(g.Items, RepositoryCellView.Key, 64f) {
                    Header = new TableSectionHeader(() => CreateHeaderView(g.Name), 26f)
                }))
                .Subscribe(x => source.Data = x.ToList());
        }

        protected override void HandleNavigation(CodeHub.Core.ViewModels.IBaseViewModel viewModel, UIViewController view)
        {
            if (view is LanguagesViewController)
            {
                var ctrlToPresent = new ThemedNavigationController(view);
                ctrlToPresent.TransitioningDelegate = new SlideDownTransition();
                PresentViewController(ctrlToPresent, true, null);
                view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done, (s, e) => DismissViewController(true, null));
                viewModel.RequestDismiss.Subscribe(_ => DismissViewController(true, null));
            }
            else
            {
                base.HandleNavigation(viewModel, view);
            }
        }

        private static UILabel CreateHeaderView(string name)
        {
            return new UILabel(new CGRect(0, 0, 320f, 26f)) 
            {
                BackgroundColor = Theme.PrimaryNavigationBarColor,
                Text = name,
                Font = UIFont.BoldSystemFontOfSize(14f),
                TextColor = Theme.PrimaryNavigationBarTextColor,
                TextAlignment = UITextAlignment.Center
            };
        }
    }
}

