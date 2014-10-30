using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Repositories;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.iOS.TableViewSources;
using System.Drawing;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.Services;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesTrendingView : ReactiveTableViewController<RepositoriesTrendingViewModel>
    {
        private readonly IThemeService _themeService;

        public RepositoriesTrendingView(IThemeService themeService)
        {
            _themeService = themeService;

            var titleButton = new TrendingTitleButton
            {
                Frame = new RectangleF(0, 0, 200f, 32f),
                TintColor = themeService.CurrentTheme.NavigationBarTextColor
            };
            titleButton.TouchUpInside += (sender, e) => ViewModel.GoToLanguages.ExecuteIfCan();
            NavigationItem.TitleView = titleButton;

            var source = new RepositoryTableViewSource(TableView);
            TableView.Source = source;

            this.WhenViewModel(x => x.Repositories)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    source.Data = x.Select(g => new TableSectionInformation<RepositoryItemViewModel, RepositoryCellView>(g.Items, RepositoryCellView.Key, 64f) {
                        Header = new TableSectionHeader(() => CreateHeaderView(g.Name), 26f)
                    }).ToList();
                });

            this.WhenViewModel(x => x.SelectedLanguage)
                .Subscribe(x => titleButton.Text = x.Name);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.ShadowImage = new UIImage();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.NavigationBar.ShadowImage = null;
        }

        private UILabel CreateHeaderView(string name)
        {
            var v = new UILabel(new RectangleF(0, 0, 320f, 26f)) { BackgroundColor = _themeService.CurrentTheme.NavigationBarColor };
            v.Text = name;
            v.Font = UIFont.BoldSystemFontOfSize(14f);
            v.TextColor = _themeService.CurrentTheme.NavigationBarTextColor;
            v.TextAlignment = UITextAlignment.Center;
            return v;
        }
    }
}

