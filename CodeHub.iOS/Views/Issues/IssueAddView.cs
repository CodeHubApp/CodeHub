using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using System.Linq;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueAddView : ViewModelDrivenDialogViewController
    {
        public IssueAddView()
        {
            Title = "New Issue";
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

            var vm = (IssueAddViewModel)ViewModel;
            var saveButton = new UIBarButtonItem(UIBarButtonSystemItem.Save);
            NavigationItem.RightBarButtonItem = saveButton;

            var title = new EntryElement("Title", string.Empty, string.Empty);
            var assignedTo = new StringElement("Responsible", "Unassigned", UITableViewCellStyle.Value1);
            var milestone = new StringElement("Milestone", "None", UITableViewCellStyle.Value1);
            var labels = new StringElement("Labels", "None", UITableViewCellStyle.Value1);
            var content = new MultilinedElement("Description");

            Root.Reset(new Section { title, assignedTo, milestone, labels }, new Section { content });

            OnActivation(d =>
            {
                d(vm.Bind(x => x.IssueTitle, true).Subscribe(x => title.Value = x));
                d(title.Changed.Subscribe(x => vm.IssueTitle = x));

                d(vm.Bind(x => x.Content, true).Subscribe(x => content.Details = x));
                d(labels.Clicked.Subscribe(_ => vm.GoToLabelsCommand.Execute(null)));
                d(milestone.Clicked.Subscribe(_ => vm.GoToMilestonesCommand.Execute(null)));
                d(assignedTo.Clicked.Subscribe(_ => vm.GoToAssigneeCommand.Execute(null)));
                d(vm.Bind(x => x.IsSaving).SubscribeStatus("Saving..."));

                d(vm.Bind(x => x.AssignedTo, true).Subscribe(x => {
                    assignedTo.Value = x == null ? "Unassigned" : x.Login;
                }));

                d(vm.Bind(x => x.Milestone, true).Subscribe(x => {
                    milestone.Value = x == null ? "None" : x.Title;
                }));

                d(vm.BindCollection(x => x.Labels, true).Subscribe(_ => {
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

