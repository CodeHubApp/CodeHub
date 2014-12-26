using System;
using System.Linq;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using CodeHub.Core.Services;
using Xamarin.Utilities.Services;
using Xamarin.Utilities.Delegates;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueAddView : ReactiveTableViewController<IssueAddViewModel>
	{
	    public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            var source = new DialogTableViewSource(TableView, true);
            TableView.Source = source;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Images.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => {
				View.EndEditing(true);
				ViewModel.SaveCommand.Execute(null);
			});

			var title = new InputElement("Title", string.Empty, string.Empty);
            title.Changed += (sender, e) => ViewModel.Subject = title.Value;

			var assignedTo = new StyledStringElement("Responsible", "Unassigned", UITableViewCellStyle.Value1);
			assignedTo.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			assignedTo.Tapped += () => ViewModel.GoToAssigneeCommand.Execute(null);

			var milestone = new StyledStringElement("Milestone", "None", UITableViewCellStyle.Value1);
			milestone.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			milestone.Tapped += () => ViewModel.GoToMilestonesCommand.Execute(null);

			var labels = new StyledStringElement("Labels", "None", UITableViewCellStyle.Value1);
			labels.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			labels.Tapped += () => ViewModel.GoToLabelsCommand.Execute(null);

			var content = new MultilinedElement("Description");
		    content.Tapped += () => ViewModel.GoToDescriptionCommand.ExecuteIfCan();

			ViewModel.WhenAnyValue(x => x.Title).Subscribe(x => title.Value = x);
            ViewModel.WhenAnyValue(x => x.Content).Subscribe(x => content.Value = x);
            ViewModel.WhenAnyValue(x => x.AssignedTo).Subscribe(x =>
            {
				assignedTo.Value = x == null ? "Unassigned" : x.Login;
                source.Root.Reload(assignedTo, UITableViewRowAnimation.None);
			});
            ViewModel.WhenAnyValue(x => x.Milestone).Subscribe(x =>
            {
				milestone.Value = x == null ? "None" : x.Title;
                source.Root.Reload(milestone, UITableViewRowAnimation.None);
			});
			ViewModel.WhenAnyValue(x => x.Labels).Subscribe(x => {
				labels.Value = (ViewModel.Labels == null || ViewModel.Labels.Length == 0) ? 
                                "None" : string.Join(", ", ViewModel.Labels.Select(i => i.Name));
                source.Root.Reload(labels, UITableViewRowAnimation.None);
			});

            source.Root.Reset(new Section { title, assignedTo, milestone, labels }, new Section { content });
		}
    }
}

