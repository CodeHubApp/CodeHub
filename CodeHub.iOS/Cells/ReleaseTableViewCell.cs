using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Releases;

namespace CodeHub.iOS.Cells
{
    public class ReleaseTableViewCell : ReactiveTableViewCell<ReleaseItemViewModel>
    {
        public static NSString Key = new NSString("release");
        private const float ImageSpacing = 10f;

        [Export("initWithStyle:reuseIdentifier:")]
        public ReleaseTableViewCell(UITableViewCellStyle style, NSString reuseIdentifier)
            : base(UITableViewCellStyle.Value1, reuseIdentifier)
        { 
            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    TextLabel.Text = x.Name;
                    DetailTextLabel.Text = x.Created;
                });

            DetailTextLabel.Font = UIFont.PreferredFootnote;
        }
    }
}

