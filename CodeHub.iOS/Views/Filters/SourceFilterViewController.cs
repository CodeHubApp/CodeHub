using System;
using UIKit;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.Filters;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Filters
{
    public class SourceFilterViewController : FilterViewController
    {
        private EnumChoiceElement<SourceFilterModel.Order> _orderby;
        private BooleanElement _ascendingElement;
        private readonly IFilterableViewModel<SourceFilterModel> _filterController;

        public SourceFilterViewController(IFilterableViewModel<SourceFilterModel> filterController)
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

            var save = new StringElement("Save as Default") { Accessory = UITableViewCellAccessory.None };
            save.Clicked.Subscribe(_ =>
            {
                _filterController.ApplyFilter(CreateFilterModel(), true);
                CloseViewController();
            });

            //Load the root
            var root = new [] {
                new Section("Order By") {
                    (_orderby = CreateEnumElement("Type", currentModel.OrderBy)),
                    (_ascendingElement = new BooleanElement("Ascending", currentModel.Ascending)),
                },
                new Section {
                    save,
                }
            };

            Root.Reset(root);
        }

        private SourceFilterModel CreateFilterModel()
        {
            var model = new SourceFilterModel {OrderBy = _orderby.Value, Ascending = _ascendingElement.Value};
            return model;
        }
    }
}

