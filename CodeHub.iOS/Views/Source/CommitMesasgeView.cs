using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Source
{
    public class CommitMesasgeView : MessageComposerViewController<CommitMessageViewModel>
    {
        public CommitMesasgeView()
        {
            TextView.Font = UIFont.SystemFontOfSize(16f);
            TextView.Changed += (sender, e) => ViewModel.CommitMessage = Text;
            this.WhenViewModel(x => x.CommitMessage).Subscribe(x => TextView.Text = x);
            this.WhenViewModel(x => x.SaveCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = x.ToBarButtonItem(UIBarButtonSystemItem.Save));
        }
    }
}

