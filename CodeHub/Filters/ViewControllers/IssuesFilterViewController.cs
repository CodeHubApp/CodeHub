using CodeFramework.Controllers;
using CodeHub.Filters.Models;
using MonoTouch.Dialog;
using CodeFramework.Filters.Controllers;
using MonoTouch.UIKit;

namespace CodeHub.Filters.ViewControllers
{
    public class IssuesFilterViewController : FilterViewController
    {
        private readonly IFilterController<IssuesFilterModel> _filterController;

        private TrueFalseElement _open;
        private EntryElement _labels;
        private EnumChoiceElement<IssuesFilterModel.Sort> _sort;
        private TrueFalseElement _asc;


        public IssuesFilterViewController(IFilterController<IssuesFilterModel> filterController)
        {
            _filterController = filterController;
        }

        public override void ApplyButtonPressed()
        {
            _filterController.ApplyFilter(CreateFilterModel());
        }

        private IssuesFilterModel CreateFilterModel()
        {
            var model = new IssuesFilterModel();
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

