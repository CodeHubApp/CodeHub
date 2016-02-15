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
		public override void ViewDidLoad()
		{
			Title = "New Issue";

			base.ViewDidLoad();

			var vm = (IssueAddViewModel)ViewModel;

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => {
				View.EndEditing(true);
				vm.SaveCommand.Execute(null);
			});

			var title = new InputElement("Title", string.Empty, string.Empty);
            title.Changed.Subscribe(x => vm.Title = x);

			var assignedTo = new StringElement("Responsible", "Unassigned", UITableViewCellStyle.Value1);
			assignedTo.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            assignedTo.Clicked.Subscribe(_ => vm.GoToAssigneeCommand.Execute(null));

			var milestone = new StringElement("Milestone", "None", UITableViewCellStyle.Value1);
			milestone.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            milestone.Clicked.Subscribe(_ => vm.GoToMilestonesCommand.Execute(null));

			var labels = new StringElement("Labels", "None", UITableViewCellStyle.Value1);
			labels.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            labels.Clicked.Subscribe(_ => vm.GoToLabelsCommand.Execute(null));

			var content = new MultilinedElement("Description");
			content.Tapped += () =>
			{
                var composer = new MarkdownComposerViewController { Title = "Issue Description", Text = content.Value };
				composer.NewComment(this, (text) => {
					vm.Content = text;
					composer.CloseComposer();
				});
			};

            vm.Bind(x => x.Title).Subscribe(x => title.Value = x);
            vm.Bind(x => x.Content).Subscribe(x => content.Value = x);
            vm.Bind(x => x.AssignedTo).Subscribe(x => {
				assignedTo.Value = x == null ? "Unassigned" : x.Login;
				Root.Reload(assignedTo, UITableViewRowAnimation.None);
			});
            vm.Bind(x => x.Milestone).Subscribe(x => {
				milestone.Value = x == null ? "None" : x.Title;
				Root.Reload(milestone, UITableViewRowAnimation.None);
			});
			vm.BindCollection(x => x.Labels, x => {
				labels.Value = vm.Labels.Items.Count == 0 ? "None" : string.Join(", ", vm.Labels.Items.Select(i => i.Name));
				Root.Reload(labels, UITableViewRowAnimation.None);
			});

            vm.Bind(x => x.IsSaving).SubscribeStatus("Saving...");
            Root.Reset(new Section { title, assignedTo, milestone, labels }, new Section { content });
		}
    }
}

