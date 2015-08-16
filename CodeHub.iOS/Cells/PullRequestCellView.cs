using System;
using Foundation;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.PullRequests;

namespace CodeHub.iOS.Cells
{
    public partial class PullRequestCellView : ReactiveTableViewCell<PullRequestItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("PullRequestCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("PullRequestCellView");
        private bool _isFakeCell;

        public PullRequestCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static PullRequestCellView Create(bool isFakeCell = false)
        {
            var cell = Nib.Instantiate(null, null).GetValue(0) as PullRequestCellView;
            cell._isFakeCell = isFakeCell;
            return cell;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();

            TitleLabel.PreferredMaxLayoutWidth = TitleLabel.Frame.Width;
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
            TimeLabel.TextColor = Theme.MainTextColor;

            this.OneWayBind(ViewModel, x => x.Title, x => x.TitleLabel.Text);
            this.OneWayBind(ViewModel, x => x.Details, x => x.TimeLabel.Text);

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Where(_ => !_isFakeCell)
                .Subscribe(x => MainImageView.SetAvatar(x));
        }
    }
}

