using System;
using Foundation;
using UIKit;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using System.Reactive.Linq;
using Humanizer;

namespace CodeHub.iOS.Cells
{
    public partial class GistCellView : ReactiveTableViewCell<GistItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("GistCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("GistCellView");
        private static nfloat DefaultContentConstraintSize = 0.0f;
        private bool _isFakeCell;

        public GistCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static GistCellView Create(bool isFakeCell = false)
        {
            var cell = Nib.Instantiate(null, null).GetValue(0) as GistCellView;
            cell._isFakeCell = isFakeCell;
            return cell;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();

            TitleLabel.PreferredMaxLayoutWidth = TitleLabel.Frame.Width;
            ContentLabel.PreferredMaxLayoutWidth = ContentLabel.Frame.Width;
            TimeLabel.PreferredMaxLayoutWidth = TimeLabel.Frame.Width;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            ContentView.Opaque = true;

            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            TitleLabel.TextColor = Theme.MainTitleColor;
            TimeLabel.TextColor = Theme.MainSubtitleColor;
            ContentLabel.TextColor = Theme.MainTextColor;
            DefaultContentConstraintSize = ContentConstraint.Constant;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    TitleLabel.Text = x.Title;
                    ContentLabel.Text = x.Description;
                    TimeLabel.Text = x.UpdatedString;
                    ContentConstraint.Constant = string.IsNullOrEmpty(x.Description) ? 0f : DefaultContentConstraintSize;
                });

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Where(_ => !_isFakeCell)
                .Subscribe(x => MainImageView.SetAvatar(x));
        }
    }
}

