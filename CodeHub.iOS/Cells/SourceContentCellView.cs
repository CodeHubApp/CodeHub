using System;
using System.Reactive.Linq;
using MonoTouch.Foundation;
using CodeHub.Core.ViewModels.Source;

namespace CodeHub.iOS.Cells
{
    public class SourceContentCellView : ReactiveTableViewCell<SourceItemViewModel>
    {
        public static NSString Key = new NSString("content");

        public SourceContentCellView(IntPtr handle)
            : base(handle)
        {
            this.WhenViewModel(x => x.Name)
                .Subscribe(x => TextLabel.Text = x);

            this.WhenViewModel(x => x.Type)
                .Subscribe(x =>
                {
                    if (x == SourceItemType.Directory)
                        ImageView.Image = Images.Directory;
                    else if (x == SourceItemType.Submodule)
                        ImageView.Image = Images.Submodule;
                    else
                        ImageView.Image = Images.FileCode;
                });
        }
    }
}

