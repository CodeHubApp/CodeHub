using System;
using GitHubSharp.Models;
using ReactiveUI;
using System.Reactive.Linq;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Cells
{
    public class TagCellView : ReactiveTableViewCell<TagModel>
    {
        public static NSString Key = new NSString("TagCell");

        public TagCellView(IntPtr handle)
            : base(handle)
        {
            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    TextLabel.Text = x.Name;
                });
        }
    }
}

