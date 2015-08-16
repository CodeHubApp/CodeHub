using System;
using Foundation;
using UIKit;
using CodeHub.Core.ViewModels.App;
using ReactiveUI;
using System.Reactive.Linq;
using Humanizer;
using SDWebImage;

namespace CodeHub.iOS.Cells
{
    public partial class FeedbackCellView : ReactiveTableViewCell<FeedbackItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("FeedbackCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("FeedbackCellView");
        private static nfloat DefaultContentConstraintSize = 0.0f;
        private bool _isFakeCell;

        public FeedbackCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static FeedbackCellView Create(bool isFakeCell = false)
        {
            var cell = Nib.Instantiate(null, null).GetValue(0) as FeedbackCellView;
            cell._isFakeCell = isFakeCell;
            return cell;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            ContentView.Opaque = true;

            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            TitleLabel.TextColor = Theme.MainTitleColor;
            DetailsLabel.TextColor = UIColor.Gray;
            DefaultContentConstraintSize = DetailsConstraint.Constant;

            this.OneWayBind(ViewModel, x => x.Title, x => x.TitleLabel.Text);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x => {
                    DetailsLabel.Text = "Created " + x.Created.UtcDateTime.Humanize();
                    DetailsConstraint.Constant = string.IsNullOrEmpty(DetailsLabel.Text) ? 0f : DefaultContentConstraintSize;
                });

            this.WhenAnyValue(x => x.ViewModel.ImageUrl)
                .Where(_ => !_isFakeCell)
                .Subscribe(x => {
                    if (string.IsNullOrEmpty(x))
                        MainImageView.Image = Images.LoginUserUnknown;
                    else
                        MainImageView.SetImage(new NSUrl(x), Images.LoginUserUnknown);
                });
        }
    }
}

