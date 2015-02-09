using System;
using System.Reactive.Linq;
using Foundation;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;

namespace CodeHub.iOS.Cells
{
    public class SourceContentCellView : ReactiveTableViewCell<SourceItemViewModel>
    {
        public static NSString Key = new NSString("content");

        public SourceContentCellView(IntPtr handle)
            : base(handle)
        {
            this.OneWayBind(ViewModel, x => x.Name, x => x.TextLabel.Text);

            this.WhenViewModel(x => x.Type)
                .Subscribe(x =>
                {
                    if (x == SourceItemType.Directory)
                        ImageView.Image = Octicon.FileDirectory.ToImage();
                    else if (x == SourceItemType.Submodule)
                        ImageView.Image = Octicon.FileSubmodule.ToImage();
                    else
                        ImageView.Image = Octicon.FileCode.ToImage();
                });
        }
    }
}

