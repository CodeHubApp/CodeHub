using System;
using Foundation;
using UIKit;
using CodeHub.Core.Utilities;
using ObjCRuntime;
using Humanizer;

namespace CodeHub.iOS.Cells
{
    public partial class PullRequestCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("PullRequestCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("PullRequestCellView");

        public PullRequestCellView()
        {
        }

        public PullRequestCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static PullRequestCellView Create()
        {
            var cell = new PullRequestCellView();
            var views = NSBundle.MainBundle.LoadNib("PullRequestCellView", cell, null);
            return Runtime.GetNSObject( views.ValueAt(0) ) as PullRequestCellView;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            ContentView.Opaque = true;

            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            TitleLabel.TextColor = Theme.CurrentTheme.MainTitleColor;
            TimeLabel.TextColor = Theme.CurrentTheme.MainTextColor;
        }

        public void Set(string title, DateTimeOffset time, GitHubAvatar avatar)
        {
            TitleLabel.Text = title;
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

