using System;
using System.Reactive.Linq;
using Foundation;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using UIKit;

namespace CodeHub.iOS.Cells
{
    public class SourceContentCellView : ReactiveTableViewCell<SourceItemViewModel>
    {
        public static NSString Key = new NSString("content");

        public SourceContentCellView(IntPtr handle)
            : base(handle)
        {
            // Try and save a little memory & performance
            var fileImage = new Lazy<UIImage>(() => Octicon.FileCode.ToImage());
            var dirImage = new Lazy<UIImage>(() => Octicon.FileDirectory.ToImage());
            var subImage = new Lazy<UIImage>(() => Octicon.FileSubmodule.ToImage());

            this.OneWayBind(ViewModel, x => x.Name, x => x.TextLabel.Text);

            this.WhenViewModel(x => x.Type)
                .Subscribe(x =>
                {
                    if (x == SourceItemType.Directory)
                        ImageView.Image = dirImage.Value;
                    else if (x == SourceItemType.Submodule)
                        ImageView.Image = subImage.Value;
                    else
                        ImageView.Image = fileImage.Value;
                });
        }
    }
}

