using System;
using UIKit;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Filters;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Filters
{
    public class IssuesFilterViewController : FilterViewController
    {
        private readonly IFilterableViewModel<IssuesFilterModel> _filterController;
        private readonly string _user;
        private readonly string _repo;
        private IssuesFilterModel.MilestoneKeyValue _milestoneHolder;

        private BooleanElement _open;
        private StringElement _milestone;
        private EntryElement _labels, _mentioned, _creator, _assignee;
        private EnumChoiceElement<IssuesFilterModel.Sort> _sort;
        private BooleanElement _asc;

        public IssuesFilterViewController(string user, string repo, IFilterableViewModel<IssuesFilterModel> filterController)
        {
            _user = user;
            _repo = repo;
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
            model.Mentioned = _mentioned.Value;
            model.Creator = _creator.Value;
            model.Assignee = _assignee.Value;
            model.Milestone = _milestoneHolder;
            return model;
        }

        private void RefreshMilestone()
        {
            if (_milestoneHolder == null)
                _milestone.Value = "None";
            else
                _milestone.Value = _milestoneHolder.Name;
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
            var root = new [] {
                new Section("Filter") {
                    (_open = new BooleanElement("Open?", model.Open)),
                    (_labels = new EntryElement("Labels", "bug,ui,@user", model.Labels) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                    (_mentioned = new EntryElement("Mentioned", "User", model.Mentioned) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                    (_creator = new EntryElement("Creator", "User", model.Creator) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                    (_assignee = new EntryElement("Assignee", "User", model.Assignee) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                    (_milestone = new StringElement("Milestone", "None", UITableViewCellStyle.Value1)),
                },
                new Section("Order By") {
                    (_sort = CreateEnumElement("Field", model.SortType)),
                    (_asc = new BooleanElement("Ascending", model.Ascending))
                },
                new Section(string.Empty, "Saving this filter as a default will save it only for this repository.") {
                    save
                }
            };

            RefreshMilestone();

            _milestone.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            _milestone.Clicked.Subscribe(_ => {
                var ctrl = new IssueMilestonesFilterViewController(_user, _repo, _milestoneHolder != null);

                ctrl.MilestoneSelected = (title, num, val) => {
                    if (title == null && num == null && val == null)
                        _milestoneHolder = null;
                    else
                        _milestoneHolder = new IssuesFilterModel.MilestoneKeyValue { Name = title, Value = val, IsMilestone = num.HasValue };
                    RefreshMilestone();
                    NavigationController.PopViewController(true);
                };
                NavigationController.PushViewController(ctrl, true);
            });

            Root.Reset(root);
        }
    }
}

