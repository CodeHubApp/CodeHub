using System;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Gists
{
    public class GistCreateView : GistModifyView<GistCreateViewModel>
    {
        private readonly BooleanElement _publicElement;

        public GistCreateView()
        {
            _publicElement = new BooleanElement("Public", false, (e) => ViewModel.IsPublic = e.Value);
            this.WhenAnyValue(x => x.ViewModel.IsPublic).Subscribe(x => _publicElement.Value = x);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Source.Root[Source.Root.Count - 1].Insert(0, _publicElement);
        }
    }
}

