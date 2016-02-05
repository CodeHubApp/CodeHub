using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.iOS.Views;
using MonoTouch.Dialog;
using System.Linq;
using CodeFramework.iOS.Utils;
using CodeHub.iOS.ViewControllers;

namespace CodeHub.iOS.Views.Issues
{
	public class IssueAddView : ViewModelDrivenDialogViewController
    {
		private IHud _hud;

		public override void ViewDidLoad()
		{
			Title = "New Issue";

			base.ViewDidLoad();

			_hud = this.CreateHud();
			var vm = (IssueAddViewModel)ViewModel;

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => {
				View.EndEditing(true);
				vm.SaveCommand.Execute(null);
			});

			var title = new InputElement("Title", string.Empty, string.Empty);
			title.Changed += (object sender, EventArgs e) => vm.Title = title.Value;

			var assignedTo = new StyledStringElement("Responsible", "Unassigned", UITableViewCellStyle.Value1);
			assignedTo.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			assignedTo.Tapped += () => vm.GoToAssigneeCommand.Execute(null);

			var milestone = new StyledStringElement("Milestone".t(), "None", UITableViewCellStyle.Value1);
			milestone.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			milestone.Tapped += () => vm.GoToMilestonesCommand.Execute(null);

			var labels = new StyledStringElement("Labels".t(), "None", UITableViewCellStyle.Value1);
			labels.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			labels.Tapped += () => vm.GoToLabelsCommand.Execute(null);

			var content = new MultilinedElement("Description");
			content.Tapped += () =>
			{
                var composer = new MarkdownComposerViewController { Title = "Issue Description", Text = content.Value };
				composer.NewComment(this, (text) => {
					vm.Content = text;
					composer.CloseComposer();
				});
			};

			vm.Bind(x => x.Title, x => title.Value = x);
			vm.Bind(x => x.Content, x => content.Value = x);
			vm.Bind(x => x.AssignedTo, x => {
				assignedTo.Value = x == null ? "Unassigned" : x.Login;
				Root.Reload(assignedTo, UITableViewRowAnimation.None);
			});
			vm.Bind(x => x.Milestone, x => {
				milestone.Value = x == null ? "None" : x.Title;
				Root.Reload(milestone, UITableViewRowAnimation.None);
			});
			vm.BindCollection(x => x.Labels, x => {
				labels.Value = vm.Labels.Items.Count == 0 ? "None" : string.Join(", ", vm.Labels.Items.Select(i => i.Name));
				Root.Reload(labels, UITableViewRowAnimation.None);
			});

			vm.Bind(x => x.IsSaving, x =>
			{
				if (x)
					_hud.Show("Saving...");
				else
					_hud.Hide();
			});

			Root = new RootElement(Title) { new Section { title, assignedTo, milestone, labels }, new Section { content } };
		}
    }
}

