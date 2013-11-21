using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Source
{
    public class TagsView : ViewModelCollectionDrivenViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Tags".t();
            NoItemsText = "No Tags".t();
            EnableSearch = true;

            base.ViewDidLoad();

            var vm = (TagsViewModel) ViewModel;
            BindCollection(vm.Tags, x => new StyledStringElement(x.Name, () => vm.GoToSourceCommand.Execute(x)));
        }
    }
}

