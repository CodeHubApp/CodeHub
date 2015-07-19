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

            var publicElement = new BooleanElement("Public", false, (e) => ViewModel.IsPublic = e.Value);
            this.WhenAnyValue(x => x.ViewModel.IsPublic).Subscribe(x => publicElement.Value = x);

            Source.Root[Source.Root.Count - 1].Insert(0, publicElement);
        }
    }
}

