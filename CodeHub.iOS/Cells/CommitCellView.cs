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

            TitleLabel.TextColor = Theme.MainTitleColor;
            TimeLabel.TextColor = UIColor.Gray;
            ContentLabel.TextColor = Theme.MainTextColor;
            DefaultContentConstraintSize = ContentConstraint.Constant;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .SubscribeSafe(x => {
                    ContentConstraint.Constant = string.IsNullOrEmpty(x.Description) ? 0f : DefaultContentConstraintSize;
                    TitleLabel.Text = x.Name;
                    ContentLabel.Text = x.Description;
                    TimeLabel.Text = x.Time;
                    MainImageView.SetAvatar(x.Avatar);
                });
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseDesignerOutlets();
            base.Dispose(disposing);
        }
    }
}

