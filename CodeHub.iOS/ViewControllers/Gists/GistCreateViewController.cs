using System;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class GistCreateViewController : GistModifyViewController<GistCreateViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var publicElement = new BooleanElement("Public", false);
            Source.Root[Source.Root.Count - 1].Insert(0, publicElement);

            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.IsPublic).Subscribe(x => publicElement.Value = x));
                d(publicElement.Changed.Subscribe(x => ViewModel.IsPublic = x));
            });
        }
    }
}

