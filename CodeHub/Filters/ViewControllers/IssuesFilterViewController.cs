using CodeFramework.Controllers;
using CodeHub.Filters.Models;
using MonoTouch.Dialog;
using CodeFramework.Filters.Controllers;
using MonoTouch.UIKit;

namespace CodeHub.Filters.ViewControllers
{
    public class IssuesFilterViewController : FilterViewController
    {
        private EntryElement _assignedTo;
        private EntryElement _reportedBy;
        private MultipleChoiceElement<IssuesFilterModel.StatusModel> _statusChoice;
        private MultipleChoiceElement<IssuesFilterModel.KindModel> _kindChoice;
        private MultipleChoiceElement<IssuesFilterModel.PriorityModel> _priorityChoice;
        private EnumChoiceElement<IssuesFilterModel.Order> _orderby;
        private readonly IFilterController<IssuesFilterModel> _filterController;

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
            model.AssignedTo = _assignedTo.Value;
            model.ReportedBy = _reportedBy.Value;
            model.Status = _statusChoice.Obj;
            model.Priority = _priorityChoice.Obj;
            model.Kind = _kindChoice.Obj;
            model.OrderBy = _orderby.Value;
            return model;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var issuesFilterModel = _filterController.Filter.Clone();

            //Load the root
            var root = new RootElement(Title) {
                new Section("Filter") {
                    (_assignedTo = new InputElement("Assigned To", "Anybody", issuesFilterModel.AssignedTo) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                    (_reportedBy = new InputElement("Reported By", "Anybody", issuesFilterModel.ReportedBy) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                    (_kindChoice = CreateMultipleChoiceElement("Kind", issuesFilterModel.Kind)),
                    (_statusChoice = CreateMultipleChoiceElement("Status", issuesFilterModel.Status)),
                    (_priorityChoice = CreateMultipleChoiceElement("Priority", issuesFilterModel.Priority)),
                },
                new Section("Order By") {
                    (_orderby = CreateEnumElement("Field", issuesFilterModel.OrderBy)),
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

