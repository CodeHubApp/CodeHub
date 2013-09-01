using CodeFramework.Filters.Controllers;
using MonoTouch.Dialog;
using CodeHub.Filters.Models;
using MonoTouch.UIKit;
using CodeFramework.Controllers;

namespace CodeHub.Filters.ViewControllers
{
    public class SourceFilterViewController : FilterViewController
    {
        private EnumChoiceElement<SourceFilterModel.Order> _orderby;
        private TrueFalseElement _ascendingElement;
        private readonly IFilterController<SourceFilterModel> _filterController;

        public SourceFilterViewController(IFilterController<SourceFilterModel> filterController)
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
                    (_orderby = CreateEnumElement("Type", currentModel.OrderBy)),
                    (_ascendingElement = new TrueFalseElement("Ascending", currentModel.Ascending)),
                },
                new Section {
                    new StyledStringElement("Save as Default", () => {
                        _filterController.ApplyFilter(CreateFilterModel(), true);
                        CloseViewController();
                    }, Images.Size) { Accessory = UITableViewCellAccessory.None },
                }
            };

            Root = root;
        }

        private SourceFilterModel CreateFilterModel()
        {
            var model = new SourceFilterModel {OrderBy = _orderby.Value, Ascending = _ascendingElement.Value};
            return model;
        }
    }
}

