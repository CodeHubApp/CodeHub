using System;
using System.Linq;
using CodeHub.iOS.ViewControllers;
using UIKit;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;
using ReactiveUI;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueEditView : DialogViewController
    {
        public IssueEditViewModel ViewModel { get; set; }

        public IssueEditView()
        {
            Title = "Edit Issue";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

            var vm = ViewModel;

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
                d(vm.WhenAnyValue(x => x.IssueTitle).Subscribe(x => title.Value = x));
                d(title.Changed.Subscribe(x => vm.IssueTitle = x));

                //d(assignedTo.Clicked.BindCommand(vm.GoToAssigneeCommand));
                //d(milestone.Clicked.BindCommand(vm.GoToMilestonesCommand));
                //d(labels.Clicked.BindCommand(vm.GoToLabelsCommand));

                d(vm.WhenAnyValue(x => x.IsOpen).Subscribe(x => state.Value = x));
                d(vm.WhenAnyValue(x => x.IsSaving).SubscribeStatus("Updating..."));

                d(state.Changed.Subscribe(x => vm.IsOpen = x));
                d(vm.WhenAnyValue(x => x.Content).Subscribe(x => content.Details = x));

                d(vm.WhenAnyValue(x => x.AssignedTo).Subscribe(x => {
                    assignedTo.Value = x == null ? "Unassigned" : x.Login;
                }));

                d(vm.WhenAnyValue(x => x.Milestone).Subscribe(x => {
                    milestone.Value = x == null ? "None" : x.Title;
                }));

                //d(vm.BindCollection(x => x.Labels, true).Subscribe(x => {
                //    labels.Value = vm.Labels.Items.Count == 0 ? "None" : string.Join(", ", vm.Labels.Items.Select(i => i.Name));
                //}));

                d(saveButton.GetClickedObservable().Subscribe(_ => {
                    View.EndEditing(true);
                    vm.SaveCommand.ExecuteNow();
                }));

                d(content.Clicked.Subscribe(_ => {
                    var composer = new MarkdownComposerViewController
                    {
                        Title = "Issue Description",
                        Text = content.Details
                    };

                    composer.Saved.Subscribe(__ =>
                    {
                        ViewModel.Content = composer.Text;
                        this.DismissViewController(true, null);
                    });

                    this.PresentModalViewController(composer);
                }));
            });
        }
    }
}

