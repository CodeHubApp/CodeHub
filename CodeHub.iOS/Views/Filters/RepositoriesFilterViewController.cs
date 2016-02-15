using System;
using UIKit;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.Filters;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Filters
{
    public class RepositoriesFilterViewController : FilterViewController
    {
        private BooleanElement _ascendingElement;
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

            var save = new StringElement("Save as Default") { Accessory = UITableViewCellAccessory.None };
            save.Clicked.Subscribe(_ =>
            {
                _filterController.ApplyFilter(CreateFilterModel(), true);
                CloseViewController();
            });
    
            Root.Reset(new Section("Order By")
            {
                (_orderby = CreateEnumElement("Field", currentModel.OrderBy)),
                (_ascendingElement = new BooleanElement("Ascending", currentModel.Ascending)),
            },
            new Section
            {
                save
            });
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

