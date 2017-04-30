using System;
using Foundation;
using UIKit;
using ReactiveUI;
using CodeHub.Core.ViewModels.Gists;
using System.Reactive.Linq;

namespace CodeHub.iOS.TableViewCells
{
    public partial class GistCellView : ReactiveTableViewCell<GistItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("GistCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("GistCellView");
        private static nfloat DefaultContentConstraintSize = 0.0f;

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

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
	            {
	                TitleLabel.Text = x.Title;
	                ContentLabel.Text = x.Description;
	                TimeLabel.Text = x.UpdatedString;
	                ContentConstraint.Constant = string.IsNullOrEmpty(x.Description) ? 0f : DefaultContentConstraintSize;
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

