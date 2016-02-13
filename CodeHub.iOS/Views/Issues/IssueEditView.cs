using System;
using System.Linq;
using CodeHub.iOS.ViewControllers;
using MonoTouch.Dialog;
using UIKit;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueEditView : ViewModelDrivenDialogViewController
    {
        private IHud _hud;

        public override void ViewDidLoad()
        {
            Title = "Edit Issue";

            base.ViewDidLoad();

            _hud = this.CreateHud();
            var vm = (IssueEditViewModel)ViewModel;

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

            var state = new TrueFalseElement("Open", true);
            state.ValueChanged += (sender, e) => vm.IsOpen = state.Value;

            vm.Bind(x => x.Title, x => title.Value = x, true);

            vm.Bind(x => x.Content, x => content.Value = x, true);

            vm.Bind(x => x.AssignedTo, x => {
                assignedTo.Value = x == null ? "Unassigned" : x.Login;
                if (assignedTo.GetImmediateRootElement() != null)
                    Root.Reload(assignedTo, UITableViewRowAnimation.None);
            }, true);

            vm.Bind(x => x.Milestone, x => {
                milestone.Value = x == null ? "None" : x.Title;
                if (assignedTo.GetImmediateRootElement() != null)
                    Root.Reload(milestone, UITableViewRowAnimation.None);
            }, true);

            vm.BindCollection(x => x.Labels, x => {
                labels.Value = vm.Labels.Items.Count == 0 ? "None" : string.Join(", ", vm.Labels.Items.Select(i => i.Name));
                if (assignedTo.GetImmediateRootElement() != null)
                    Root.Reload(labels, UITableViewRowAnimation.None);
            }, true);

            vm.Bind(x => x.IsOpen, x =>
            {
                if (state.Value == x)
                    return;
                state.Value = x;
                if (assignedTo.GetImmediateRootElement() != null)
                    Root.Reload(state, UITableViewRowAnimation.None);
            }, true);

            vm.Bind(x => x.IsSaving, x =>
            {
                if (x)
                    _hud.Show("Updating...");
                else
                    _hud.Hide();
            });

            Root = new RootElement(Title) { new Section { title, assignedTo, milestone, labels }, new Section { state }, new Section { content } };
        }
    }
}

