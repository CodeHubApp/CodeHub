using System;
using UIKit;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.DialogElements;
using ReactiveUI;
using Humanizer;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class RepositoryIssuesFilterViewController : BaseTableViewController<RepositoryIssuesFilterViewModel>, IModalViewController
    {
        public RepositoryIssuesFilterViewController()
            : base(UITableViewStyle.Grouped)
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

            this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                .Select(x => x.ToBarButtonItem(Images.Cancel))
                .Subscribe(x => NavigationItem.LeftBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Select(x => x.ToBarButtonItem(Images.Search))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DialogTableViewSource(TableView);
            TableView.Source = source;

            var stateElement = new StringElement("State", () => ViewModel.SelectStateCommand.ExecuteIfCan());
            stateElement.Style = UITableViewCellStyle.Value1;

            var cmd = ViewModel.SelectLabelsCommand;
            var labelElement = new StringElement("Labels", () => cmd.ExecuteIfCan());
            labelElement.Style = UITableViewCellStyle.Value1;

            var mentionedElement = new EntryElement("Mentioned", "username", string.Empty) {
                TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None
            };
            mentionedElement.Changed += (sender, e) => ViewModel.Mentioned = mentionedElement.Value;

            var creatorElement = new EntryElement("Creator", "username", string.Empty) {
                TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None
            };
            creatorElement.Changed += (sender, e) => ViewModel.Creator = creatorElement.Value;

            var assigneeElement = new StringElement("Assignee", () => ViewModel.SelectAssigneeCommand.ExecuteIfCan());
            assigneeElement.Style = UITableViewCellStyle.Value1;

            var milestoneElement = new StringElement("Milestone", () => ViewModel.SelectMilestoneCommand.ExecuteIfCan());
            milestoneElement.Style = UITableViewCellStyle.Value1;

            var fieldElement = new StringElement("Field", () => ViewModel.SelectSortCommand.ExecuteIfCan());
            fieldElement.Style = UITableViewCellStyle.Value1;

            var ascElement = new BooleanElement("Ascending", false, x => ViewModel.Ascending = x.Value);

            var filterSection = new Section("Filter") { stateElement, mentionedElement, creatorElement, labelElement, assigneeElement, milestoneElement };
            var orderSection = new Section("Order By") { fieldElement, ascElement };
            var searchSection = new Section();
            searchSection.FooterView = new TableFooterButton("Search!", () => ViewModel.SaveCommand.ExecuteIfCan());
            source.Root.Add(filterSection, orderSection, searchSection);

            this.WhenAnyObservable(x => x.ViewModel.SelectAssigneeCommand)
                .Subscribe(_ => IssueAssigneeViewController.Show(this, ViewModel.CreateAssigneeViewModel()));

            this.WhenAnyObservable(x => x.ViewModel.SelectMilestoneCommand)
                .Subscribe(_ => IssueMilestonesViewController.Show(this, ViewModel.CreateMilestonesViewModel()));

            this.WhenAnyObservable(x => x.ViewModel.SelectLabelsCommand)
                .Subscribe(_ => IssueLabelsViewController.Show(this, ViewModel.CreateLabelsViewModel()));

            this.WhenAnyValue(x => x.ViewModel.State).Subscribe(x => stateElement.Value = x.Humanize());
            this.WhenAnyValue(x => x.ViewModel.LabelsString).Subscribe(x => labelElement.Value = x);
            this.WhenAnyValue(x => x.ViewModel.Mentioned).Subscribe(x => mentionedElement.Value = x);
            this.WhenAnyValue(x => x.ViewModel.Creator).Subscribe(x => creatorElement.Value = x);
            this.WhenAnyValue(x => x.ViewModel.AssigneeString).Subscribe(x => assigneeElement.Value = x);
            this.WhenAnyValue(x => x.ViewModel.MilestoneString).Subscribe(x => milestoneElement.Value = x);
            this.WhenAnyValue(x => x.ViewModel.SortType).Subscribe(x => fieldElement.Value = x.Humanize());
            this.WhenAnyValue(x => x.ViewModel.Ascending).Subscribe(x => ascElement.Value = x);
        }
    }
}

