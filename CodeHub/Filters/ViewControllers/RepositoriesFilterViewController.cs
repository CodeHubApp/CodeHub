using CodeFramework.Filters.Controllers;
using MonoTouch.Dialog;
using CodeHub.Filters.Models;
using MonoTouch.UIKit;
using CodeFramework.Controllers;

namespace CodeHub.Filters.ViewControllers
{
    public class RepositoriesFilterViewController : FilterViewController
    {
        private TrueFalseElement _ascendingElement;
        private EnumChoiceElement<RepositoriesFilterModel.Order> _orderby;
        private readonly IFilterController<RepositoriesFilterModel> _filterController;

        public RepositoriesFilterViewController(IFilterController<RepositoriesFilterModel> filterController)
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

