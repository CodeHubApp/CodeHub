using MonoTouch.Dialog;
using UIKit;
using CodeFramework.iOS.ViewControllers;
using CodeHub.Core.Filters;
using CodeFramework.Core.ViewModels;

namespace CodeHub.iOS.Views.Filters
{
    public class RepositoriesFilterViewController : FilterViewController
    {
        private TrueFalseElement _ascendingElement;
        private EnumChoiceElement<RepositoriesFilterModel.Order> _orderby;
        private readonly IFilterableViewModel<RepositoriesFilterModel> _filterController;

        public RepositoriesFilterViewController(IFilterableViewModel<RepositoriesFilterModel> filterController)
        {
            _filterController = filterController;
        }

        public override void ApplyButtonPressed()
        {
            _filterController.ApplyFilter(CreateFilterModel());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var currentModel = _filterController.Filter.Clone();

            //Load the root
            var root = new RootElement(Title) {
                new Section("Order By") {
                    (_orderby = CreateEnumElement("Field", currentModel.OrderBy)),
                    (_ascendingElement = new TrueFalseElement("Ascending", currentModel.Ascending)),
                },
                new Section {
                    new StyledStringElement("Save as Default", () =>{
                        _filterController.ApplyFilter(CreateFilterModel(), true);
                        CloseViewController();
                    }, Images.Size) { Accessory = UITableViewCellAccessory.None },
                }
            };

            Root = root;
        }

        private RepositoriesFilterModel CreateFilterModel()
        {
            var model = new RepositoriesFilterModel {OrderBy = _orderby.Value, Ascending = _ascendingElement.Value};
            return model;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            TableView.ReloadData();
        }
    }
}

