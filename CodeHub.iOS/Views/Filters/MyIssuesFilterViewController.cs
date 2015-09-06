using MonoTouch.Dialog;
using UIKit;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;

namespace CodeHub.iOS.Views.Filters
{
    public class MyIssuesFilterViewController : FilterViewController
    {
        private readonly IFilterableViewModel<MyIssuesFilterModel> _filterController;

        private EnumChoiceElement<MyIssuesFilterModel.Filter> _filter;
        private TrueFalseElement _open;
        private EntryElement _labels;
        private EnumChoiceElement<MyIssuesFilterModel.Sort> _sort;
        private TrueFalseElement _asc;


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

            //Load the root
            var root = new RootElement(Title) {
                new Section("Filter") {
                    (_filter = CreateEnumElement("Type", model.FilterType)),
                    (_open = new TrueFalseElement("Open?", model.Open)),
                    (_labels = new InputElement("Labels", "bug,ui,@user", model.Labels) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                },
                new Section("Order By") {
                    (_sort = CreateEnumElement("Field", model.SortType)),
                    (_asc = new TrueFalseElement("Ascending", model.Ascending))
                },
                new Section() {
                    new StyledStringElement("Save as Default", () =>{
                        _filterController.ApplyFilter(CreateFilterModel(), true);
                        CloseViewController();
                    }, Images.Size) { Accessory = UITableViewCellAccessory.None },
                }
            };

            Root = root;
        }
    }
}

