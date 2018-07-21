using System;
using System.Reactive;
using System.Reactive.Linq;
using UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public class ComposerViewController : TextViewController
    {
        private readonly UIBarButtonItem _saveButton = new UIBarButtonItem(UIBarButtonSystemItem.Save);

        public IObservable<Unit> Saved => _saveButton.GetClickedObservable().Select(_ => Unit.Default);

        public bool EnableSendButton
        {
            get => _saveButton.Enabled;
            set => _saveButton.Enabled = value;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationItem.RightBarButtonItem = _saveButton;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }
    }
}
