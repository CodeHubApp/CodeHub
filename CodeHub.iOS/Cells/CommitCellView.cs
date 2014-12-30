using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Changesets;
using Humanizer;

namespace CodeHub.iOS.Cells
{
    public partial class CommitCellView : ReactiveTableViewCell<CommitItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("CommitCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("CommitCellView");
        private static float DefaultContentConstraintSize = 0.0f;

        public CommitCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static CommitCellView Create()
        {
            return Nib.Instantiate(null, null).GetValue(0) as CommitCellView;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            TitleLabel.TextColor = Theme.MainTitleColor;
            TimeLabel.TextColor = UIColor.Gray;
            ContentLabel.TextColor = Theme.MainTextColor;
            DefaultContentConstraintSize = ContentConstraint.Constant;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .SubscribeSafe(x =>
                {
                    MainImageView.SetAvatar(x.Avatar);
                    TitleLabel.Text = x.Name;
                    ContentLabel.Text = x.Description;
                    TimeLabel.Text = x.Time.UtcDateTime.Humanize();
                    ContentConstraint.Constant = string.IsNullOrEmpty(x.Description) ? 0f : DefaultContentConstraintSize;
                });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();
            ContentLabel.PreferredMaxLayoutWidth = ContentLabel.Frame.Width;
        }
    }
}

