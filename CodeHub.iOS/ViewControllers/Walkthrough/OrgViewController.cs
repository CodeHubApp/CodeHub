using UIKit;
using System;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class OrgViewController : BaseViewController
    {
        public OrgViewController()
            : base("OrgViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TellMeMoreButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            TellMeMoreButton.Layer.CornerRadius = 6f;

            OnActivation(d => d(TellMeMoreButton.GetClickedObservable().Subscribe(_ => TellMeMore())));
        }

        private void TellMeMore()
        {
            const string url = "https://help.github.com/articles/about-third-party-application-restrictions/";
            var view = new WebBrowserViewController(url);
            PresentViewController(view, true, null);
        }
    }
}

