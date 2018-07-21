using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using System.Linq;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;
using ReactiveUI;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueAddView : DialogViewController
    {
        public IssueAddViewModel ViewModel { get; }

        public IssueAddView()
        {
            Title = "New Issue";
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

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
                d(ViewModel.WhenAnyValue(x => x.IssueTitle).Subscribe(x => title.Value = x));
                d(title.Changed.Subscribe(x => ViewModel.IssueTitle = x));

                d(ViewModel.WhenAnyValue(x => x.Content).Subscribe(x => content.Details = x));
                //d(labels.Clicked.Subscribe(_ => vm.GoToLabelsCommand.Execute(null)));
                //d(milestone.Clicked.Subscribe(_ => vm.GoToMilestonesCommand.Execute(null)));
                //d(assignedTo.Clicked.Subscribe(_ => vm.GoToAssigneeCommand.Execute(null)));
                d(ViewModel.WhenAnyValue(x => x.IsSaving).SubscribeStatus("Saving..."));

                d(ViewModel.WhenAnyValue(x => x.AssignedTo).Subscribe(x => {
                    assignedTo.Value = x == null ? "Unassigned" : x.Login;
                }));

                d(ViewModel.WhenAnyValue(x => x.Milestone).Subscribe(x => {
                    milestone.Value = x == null ? "None" : x.Title;
                }));

                //d(vm.BindCollection(x => x.Labels, true).Subscribe(_ => {
                //    labels.Value = vm.Labels.Count == 0 ? "None" : string.Join(", ", vm.Labels.Items.Select(i => i.Name));
                //}));

                d(saveButton.GetClickedObservable().Subscribe(_ => {
                    View.EndEditing(true);
                    ViewModel.SaveCommand.ExecuteNow();
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

