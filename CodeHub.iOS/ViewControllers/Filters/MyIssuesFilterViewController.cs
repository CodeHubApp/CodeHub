using System;
using UIKit;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Filters;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Filters
{
    public class MyIssuesFilterViewController : FilterViewController
    {
        private readonly IFilterableViewModel<MyIssuesFilterModel> _filterController;

        private EnumChoiceElement<MyIssuesFilterModel.Filter> _filter;
        private BooleanElement _open;
        private EntryElement _labels;
        private EnumChoiceElement<MyIssuesFilterModel.Sort> _sort;
        private BooleanElement _asc;


        public MyIssuesFilterViewController(IFilterableViewModel<MyIssuesFilterModel> filterController)
        {
            _filterController = filterController;
        }

        public override void ApplyButtonPressed()
        {
            _filterController.ApplyFilter(CreateFilterModel());
        }

        private MyIssuesFilterModel CreateFilterModel()
        {
            var model = new MyIssuesFilterModel();
            model.FilterType = _filter.Value;
            model.Open = _open.Value;
            model.Labels = _labels.Value;
            model.SortType = _sort.Value;
            model.Ascending = _asc.Value;
            return model;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var model = _filterController.Filter.Clone();

            var save = new StringElement("Save as Default") { Accessory = UITableViewCellAccessory.None };
            save.Clicked.Subscribe(_ =>
            {
                _filterController.ApplyFilter(CreateFilterModel(), true);
                CloseViewController();
            });

            //Load the root
            var sections = new [] {
                new Section("Filter") {
                    (_filter = CreateEnumElement("Type", model.FilterType)),
                    (_open = new BooleanElement("Open?", model.Open)),
                    (_labels = new EntryElement("Labels", "bug,ui,@user", model.Labels) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                },
                new Section("Order By") {
                    (_sort = CreateEnumElement("Field", model.SortType)),
                    (_asc = new BooleanElement("Ascending", model.Ascending))
                },
                new Section() {
                    save,
                }
            };

            Root.Reset(sections);
        }
    }
}

