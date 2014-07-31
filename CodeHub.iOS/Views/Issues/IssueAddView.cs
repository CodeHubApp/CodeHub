using System;
using System.Linq;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Views.App;
using MonoTouch.UIKit;
using CodeFramework.iOS.Views;
using MonoTouch.Dialog;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Issues
{
	public class IssueAddView : ViewModelDialogViewController<IssueAddViewModel>
	{
	    private readonly IStatusIndicatorService _statusIndicatorService;

	    public IssueAddView(IStatusIndicatorService statusIndicatorService)
	    {
	        _statusIndicatorService = statusIndicatorService;
	    }

	    public override void ViewDidLoad()
		{
			Title = "New Issue";

			base.ViewDidLoad();

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => {
				View.EndEditing(true);
				ViewModel.SaveCommand.Execute(null);
			});

			var title = new InputElement("Title", string.Empty, string.Empty);
			title.Changed += (sender, e) => ViewModel.Title = title.Value;

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
				Root.Reload(assignedTo, UITableViewRowAnimation.None);
			});
            ViewModel.WhenAnyValue(x => x.Milestone).Subscribe(x =>
            {
				milestone.Value = x == null ? "None" : x.Title;
				Root.Reload(milestone, UITableViewRowAnimation.None);
			});
			ViewModel.WhenAnyValue(x => x.Labels).Subscribe(x => {
				labels.Value = (ViewModel.Labels == null || ViewModel.Labels.Length == 0) ? 
                                "None" : string.Join(", ", ViewModel.Labels.Select(i => i.Name));
				Root.Reload(labels, UITableViewRowAnimation.None);
			});

			ViewModel.SaveCommand.IsExecuting.Subscribe(x =>
			{
				if (x)
					_statusIndicatorService.Show("Saving...");
				else
                    _statusIndicatorService.Hide();
			});

            Root.Reset(new Section { title, assignedTo, milestone, labels }, new Section { content });
		}
    }
}

