using System;
using Foundation;
using UIKit;
using ReactiveUI;
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

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            ContentView.Opaque = true;

            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            TitleLabel.TextColor = Theme.MainTitleColor;
            TimeLabel.TextColor = Theme.MainTextColor;

            this.WhenActivated(d => {
                d(this.OneWayBind(ViewModel, x => x.Title, x => x.TitleLabel.Text));
                d(this.OneWayBind(ViewModel, x => x.Details, x => x.TimeLabel.Text));
                d(this.WhenAnyValue(x => x.ViewModel.Avatar).Subscribe(x => MainImageView.SetAvatar(x)));
            });
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseDesignerOutlets();
            base.Dispose(disposing);
        }
    }
}

