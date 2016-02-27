using System;
using Foundation;
using UIKit;
using CodeHub.Core.Utilities;
using ObjCRuntime;
using Humanizer;

namespace CodeHub.iOS.TableViewCells
{
    public partial class GistCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("GistCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("GistCellView");
        private static nfloat DefaultContentConstraintSize = 0.0f;

        public GistCellView()
        {
        }

        public GistCellView(IntPtr handle) 
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
            TimeLabel.TextColor = Theme.CurrentTheme.MainSubtitleColor;
            ContentLabel.TextColor = Theme.CurrentTheme.MainTextColor;
            DefaultContentConstraintSize = ContentConstraint.Constant;
        }

        public static GistCellView Create()
        {
            var cell = new GistCellView();
            var views = NSBundle.MainBundle.LoadNib("GistCellView", cell, null);
            return Runtime.GetNSObject( views.ValueAt(0) ) as GistCellView;
        }

        public void Set(string title, string description, DateTimeOffset time, GitHubAvatar avatar)
        {
            TitleLabel.Text = title;
            ContentLabel.Text = description;
            TimeLabel.Text = time.Humanize();
            ContentConstraint.Constant = string.IsNullOrEmpty(description) ? 0f : DefaultContentConstraintSize;
            MainImageView.SetAvatar(avatar);
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseDesignerOutlets();
            base.Dispose(disposing);
        }
    }
}

