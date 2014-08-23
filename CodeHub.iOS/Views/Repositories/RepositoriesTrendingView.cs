using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Repositories;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.iOS.TableViewSources;
using System.Drawing;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesTrendingView : ReactiveTableViewController<RepositoriesTrendingViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Trending";

            base.ViewDidLoad();

            var titleButton = new TitleButton { Frame = new RectangleF(0, 0, 320f, 44f) };
            titleButton.TouchUpInside += (sender, e) => ViewModel.GoToLanguages.ExecuteIfCan();
            ViewModel.WhenAnyValue(x => x.SelectedLanguage).Subscribe(x => titleButton.Text = x.Name);
            NavigationItem.TitleView = titleButton;

            var source = new RepositoryTableViewSource(TableView);
            TableView.Source = source;

            ViewModel.WhenAnyValue(x => x.Repositories)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    source.Data = x.Select(g =>
                    {
                        var t = new TableSectionInformation<RepositoryItemViewModel, RepositoryCellView>(g.Items, RepositoryCellView.Key, 64f);
                        t.Header = new TableSectionHeader(() => CreateHeaderView(g.Name), 26f);
                        return t;
                    }).ToList();
                });

            ViewModel.LoadCommand.ExecuteIfCan();
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
            var v = new UILabel(new RectangleF(0, 0, 320f, 26f)) { BackgroundColor = UINavigationBar.Appearance.BarTintColor };
            v.Text = name;
            v.Font = UIFont.BoldSystemFontOfSize(14f);
            v.TextColor = UINavigationBar.Appearance.TintColor;
            v.TextAlignment = UITextAlignment.Center;
            return v;
        }

        private class TitleButton : UIButton
        {
            private readonly UILabel _label;
            private readonly UIImageView _imageView;

            public string Text
            {
                get { return _label.Text; }
                set 
                { 
                    _label.Text = value;
                    SetNeedsLayout();
                }
            }

            public TitleButton()
            {
                _label = new UILabel();
                _label.TextColor = UINavigationBar.Appearance.TintColor;
                _label.TextAlignment = UITextAlignment.Center;
                Add(_label);

                _imageView = new UIImageView();
                _imageView.Frame = new System.Drawing.RectangleF(0, 0, 12, 12);
                _imageView.TintColor = UINavigationBar.Appearance.TintColor;
                _imageView.Image = Images.DownChevron.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                Add(_imageView);
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                _label.SizeToFit();
                _label.Center = new System.Drawing.PointF(Frame.Width / 2f, Frame.Height / 2f);
                _imageView.Center = new System.Drawing.PointF(_label.Frame.Right + 12f, Frame.Height / 2f);
            }
        }
    }
}

