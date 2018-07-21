using UIKit;
using System;
using System.Reactive.Linq;

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

            OnActivation(d => 
            {
                d(TellMeMoreButton
                  .GetClickedObservable()
                  .Select(_ => "https://help.github.com/articles/about-third-party-application-restrictions/")
                  .Subscribe(this.PresentSafari));
            });
        }
    }
}

