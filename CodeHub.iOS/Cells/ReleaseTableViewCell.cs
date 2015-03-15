using Foundation;
using UIKit;
using ReactiveUI;
using CodeHub.Core.ViewModels.Releases;

namespace CodeHub.iOS.Cells
{
    public class ReleaseTableViewCell : ReactiveTableViewCell<ReleaseItemViewModel>
    {
        public static NSString Key = new NSString("release");

        [Export("initWithStyle:reuseIdentifier:")]
        public ReleaseTableViewCell(UITableViewCellStyle style, NSString reuseIdentifier)
            : base(UITableViewCellStyle.Subtitle, reuseIdentifier)
        { 
            TextLabel.TextColor = Theme.MainTitleColor;
            TextLabel.Font = UIFont.PreferredBody;
            DetailTextLabel.Font = UIFont.PreferredFootnote;
            DetailTextLabel.TextColor = Theme.MainSubtitleColor;

            this.OneWayBind(ViewModel, x => x.Name, x => x.TextLabel.Text);
            this.OneWayBind(ViewModel, x => x.Created, x => x.DetailTextLabel.Text);
        }
    }
}

