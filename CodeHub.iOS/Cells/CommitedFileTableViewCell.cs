using System;
using Foundation;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Source;

namespace CodeHub.iOS.Cells
{
    public class CommitedFileTableViewCell : ReactiveTableViewCell<CommitedFileItemViewModel>
    {
        public static NSString Key = new NSString("commitedfile");

        [Export("initWithStyle:reuseIdentifier:")]
        public CommitedFileTableViewCell(UITableViewCellStyle style, NSString reuseIdentifier)
            : base(UITableViewCellStyle.Subtitle, reuseIdentifier)
        {
            ImageView.Image = Octicon.FileCode.ToImage();
            DetailTextLabel.TextColor = Theme.MainSubtitleColor;

            this.OneWayBind(ViewModel, x => x.Name, x => x.TextLabel.Text);
            this.OneWayBind(ViewModel, x => x.Subtitle, x => x.DetailTextLabel.Text);
        }
    }
}

