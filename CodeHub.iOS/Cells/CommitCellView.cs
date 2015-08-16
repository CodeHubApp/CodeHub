using System;
using Foundation;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.iOS.Cells
{
    public partial class CommitCellView : ReactiveTableViewCell<CommitItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("CommitCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("CommitCellView");
        private static nfloat DefaultContentConstraintSize = 0.0f;
        private bool _isFakeCell;

        public CommitCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static CommitCellView Create(bool isFakeCell = false)
        {
            var cell = Nib.Instantiate(null, null).GetValue(0) as CommitCellView;
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
            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            ContentView.Opaque = true;

            TitleLabel.TextColor = Theme.MainTitleColor;
            TimeLabel.TextColor = UIColor.Gray;
            ContentLabel.TextColor = Theme.MainTextColor;
            DefaultContentConstraintSize = ContentConstraint.Constant;

            this.OneWayBind(ViewModel, x => x.Name, x => x.TitleLabel.Text);
            this.OneWayBind(ViewModel, x => x.Description, x => x.ContentLabel.Text);
            this.OneWayBind(ViewModel, x => x.Time, x => x.TimeLabel.Text);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .SubscribeSafe(x => ContentConstraint.Constant = string.IsNullOrEmpty(x.Description) ? 0f : DefaultContentConstraintSize);

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Where(_ => !_isFakeCell)
                .Subscribe(x => MainImageView.SetAvatar(x));
        }
    }
}

