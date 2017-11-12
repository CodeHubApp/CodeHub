using System;
using System.Linq;
using CodeHub.iOS.ViewControllers;
using UIKit;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueEditView : ViewModelDrivenDialogViewController
    {
        public IssueEditView()
        {
            Title = "Edit Issue";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

            var vm = (IssueEditViewModel)ViewModel;

            var saveButton = new UIBarButtonItem(UIBarButtonSystemItem.Save);
            NavigationItem.RightBarButtonItem = saveButton;

            var title = new EntryElement("Title", string.Empty, string.Empty);
            var assignedTo = new StringElement("Responsible", "Unassigned", UITableViewCellStyle.Value1);
            var milestone = new StringElement("Milestone", "None", UITableViewCellStyle.Value1);
            var labels = new StringElement("Labels", "None", UITableViewCellStyle.Value1);
            var content = new MultilinedElement("Description");
            var state = new BooleanElement("Open", true);

            Root.Reset(new Section { title, assignedTo, milestone, labels }, new Section { state }, new Section { content });

            OnActivation(d =>
            {
                d(vm.Bind(x => x.IssueTitle, true).Subscribe(x => title.Value = x));
                d(title.Changed.Subscribe(x => vm.IssueTitle = x));

                d(assignedTo.Clicked.BindCommand(vm.GoToAssigneeCommand));
                d(milestone.Clicked.BindCommand(vm.GoToMilestonesCommand));
                d(labels.Clicked.BindCommand(vm.GoToLabelsCommand));

                d(vm.Bind(x => x.IsOpen, true).Subscribe(x => state.Value = x));
                d(vm.Bind(x => x.IsSaving).SubscribeStatus("Updating..."));

                d(state.Changed.Subscribe(x => vm.IsOpen = x));
                d(vm.Bind(x => x.Content, true).Subscribe(x => content.Details = x));

                d(vm.Bind(x => x.AssignedTo, true).Subscribe(x => {
                    assignedTo.Value = x == null ? "Unassigned" : x.Login;
                }));

                d(vm.Bind(x => x.Milestone, true).Subscribe(x => {
                    milestone.Value = x == null ? "None" : x.Title;
                }));

                d(vm.BindCollection(x => x.Labels, true).Subscribe(x => {
                    labels.Value = vm.Labels.Items.Count == 0 ? "None" : string.Join(", ", vm.Labels.Items.Select(i => i.Name));
                }));

                d(saveButton.GetClickedObservable().Subscribe(_ => {
                    View.EndEditing(true);
                    vm.SaveCommand.Execute(null);
                }));

                d(content.Clicked.Subscribe(_ => {
                    var composer = new MarkdownComposerViewController
                    {
                        Title = "Issue Description",
                        Text = content.Details
                    };

                    composer.PresentAsModal(this, text =>
                    {
                        vm.Content = text;
                        this.DismissViewController(true, null);
                    });
                }));
            });
        }
    }
}

