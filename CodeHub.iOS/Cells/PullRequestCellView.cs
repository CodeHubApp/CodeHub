using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.PullRequests;

namespace CodeHub.iOS.Cells
{
    public partial class PullRequestCellView : ReactiveTableViewCell<PullRequestItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("PullRequestCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("PullRequestCellView");

        public PullRequestCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static PullRequestCellView Create()
        {
            return Nib.Instantiate(null, null).GetValue(0) as PullRequestCellView;
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

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    MainImageView.SetAvatar(x.Avatar);
                    TitleLabel.Text = x.Title;
                    TimeLabel.Text = x.Details;
                });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();
            TitleLabel.PreferredMaxLayoutWidth = TitleLabel.Frame.Width;
        }
    }
}

