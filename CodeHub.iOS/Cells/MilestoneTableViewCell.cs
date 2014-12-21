using System;
using ReactiveUI;
using CodeHub.iOS.ViewComponents;
using MonoTouch.UIKit;
using System.Drawing;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Cells
{
    public class MilestoneTableViewCell : ReactiveTableViewCell<MilestoneItemViewModel>
    {
        public static NSString Key = new NSString("milestonecellview");
        private readonly MilestoneView _milestoneView;

        public override void SetSelected(bool selected, bool animated)
        {
            BackgroundColor = selected ? UIColor.FromWhiteAlpha(0.9f, 1.0f) : UIColor.White;
        }

        public override void SetHighlighted(bool highlighted, bool animated)
        {
            BackgroundColor = highlighted ? UIColor.FromWhiteAlpha(0.9f, 1.0f) : UIColor.White;
        }

        public MilestoneTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            var frame = Frame = new RectangleF(0, 0, 320f, 80);
            AutosizesSubviews = true;
            ContentView.AutosizesSubviews = true;
            SeparatorInset = UIEdgeInsets.Zero;

            _milestoneView = new MilestoneView();
            _milestoneView.Frame = frame;
            _milestoneView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            ContentView.Add(_milestoneView);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x => _milestoneView.Init(x.Title, x.OpenIssues, x.ClosedIssues, x.DueDate));
        }
    }
}

