using System;
using Foundation;
using UIKit;
using CodeHub.Core.Utilities;
using ObjCRuntime;
using Humanizer;

namespace CodeHub.iOS.TableViewCells
{
    public partial class CommitCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("CommitCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("CommitCellView");
        private static nfloat DefaultContentConstraintSize = 0.0f;

        public CommitCellView()
        {
        }

        public CommitCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static CommitCellView Create()
        {
            var cell = new CommitCellView();
            var views = NSBundle.MainBundle.LoadNib("CommitCellView", cell, null);
            return Runtime.GetNSObject( views.ValueAt(0) ) as CommitCellView;
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
        }

        public void Set(string title, string description, DateTimeOffset time, GitHubAvatar avatar)
        {
            ContentConstraint.Constant = string.IsNullOrEmpty(description) ? 0f : DefaultContentConstraintSize;
            TitleLabel.Text = title;
            ContentLabel.Text = description;
            TimeLabel.Text = time.Humanize();
            MainImageView.SetAvatar(avatar);
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseDesignerOutlets();
            base.Dispose(disposing);
        }
    }
}

