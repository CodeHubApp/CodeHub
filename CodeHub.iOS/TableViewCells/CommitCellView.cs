using Foundation;
using UIKit;
using Humanizer;
using ObjCRuntime;
using CodeHub.Core.ViewModels.Changesets;
using System;

namespace CodeHub.iOS.TableViewCells
{
    public partial class CommitCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("CommitCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("CommitCellView");
        public static readonly UIStringAttributes BoldAttribute;

        static CommitCellView()
        {
            BoldAttribute = new UIStringAttributes { Font = UIFont.PreferredSubheadline.MakeBold() };
        }

        public CommitCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static CommitCellView Create()
        {
            var views = NSBundle.MainBundle.LoadNib(Key, null, null);
            return Runtime.GetNSObject<CommitCellView>(views.ValueAt(0));
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);

            TitleLabel.TextColor = Theme.CurrentTheme.MainTitleColor;
            TimeLabel.TextColor = UIColor.Gray;
        }

        public void Set(CommitItemViewModel viewModel)
        {
            TitleLabel.Text = viewModel.Title;
            MainImageView.SetAvatar(viewModel.Avatar);

            var prettyDate = viewModel.Date?.Humanize();
            var prettyString = new NSMutableAttributedString($"{viewModel.Name} commited {prettyDate}");
            prettyString.SetAttributes(
                BoldAttribute.Dictionary,
                new NSRange(0, viewModel.Name.Length));
            prettyString.SetAttributes(
                BoldAttribute.Dictionary,
                new NSRange(prettyString.Length - prettyDate.Length, prettyDate.Length));
            TimeLabel.AttributedText = prettyString;
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseDesignerOutlets();
            base.Dispose(disposing);
        }
    }
}

