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
using System.Collections.Generic;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesTrendingView : ReactiveTableViewController<RepositoriesTrendingViewModel>
    {
        private readonly TrendingTitleButton _titleButton;

        public RepositoriesTrendingView()
        {
            NavigationItem.TitleView = _titleButton = new TrendingTitleButton
            {
                Frame = new RectangleF(0, 0, 200f, 32f),
                TintColor = Theme.PrimaryNavigationBarTextColor
            };
            _titleButton.TouchUpInside += (sender, e) => ViewModel.GoToLanguages.ExecuteIfCan();

            this.WhenAnyValue(x => x.ViewModel.SelectedLanguage).IsNotNull()
                .Subscribe(x => _titleButton.Text = x.Name);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new RepositoryTableViewSource(TableView);
            TableView.Source = source;

            this.WhenViewModel(x => x.Repositories)
                .Subscribe(x =>
                {
                    if (x == null)
                    {
                        source.Data = new List<TableSectionInformation<RepositoryItemViewModel, RepositoryCellView>>();
                    }
                    else
                    {
                        source.Data = x.Select(g => new TableSectionInformation<RepositoryItemViewModel, RepositoryCellView>(g.Items, RepositoryCellView.Key, 64f) {
                            Header = new TableSectionHeader(() => CreateHeaderView(g.Name), 26f)
                        }).ToList();
                    }
                });
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

        private static UILabel CreateHeaderView(string name)
        {
            return new UILabel(new RectangleF(0, 0, 320f, 26f)) 
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

