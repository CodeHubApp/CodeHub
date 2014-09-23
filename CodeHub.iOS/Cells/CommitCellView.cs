using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using SDWebImage;
using CodeHub.Core.ViewModels.Changesets;

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

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            TitleLabel.TextColor = Theme.CurrentTheme.MainTitleColor;
            TimeLabel.TextColor = UIColor.Gray;
            ContentLabel.TextColor = Theme.CurrentTheme.MainTextColor;
            DefaultContentConstraintSize = ContentConstraint.Constant;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    MainImageView.SetImage(new NSUrl(x.ImageUrl), Images.LoginUserUnknown);
                    TitleLabel.Text = x.Name;
                    ContentLabel.Text = x.Description;
                    TimeLabel.Text = x.Time.ToDaysAgo();
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

