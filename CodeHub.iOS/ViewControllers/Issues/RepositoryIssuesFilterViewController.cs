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
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DialogTableViewSource(TableView);
            TableView.Source = source;

            var mentionedElement = new EntryElement("Mentioned", "username", string.Empty) {
                TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None
            };

            var creatorElement = new EntryElement("Creator", "username", string.Empty) {
                TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None
            };

            var stateElement = new StringElement("State", string.Empty, UITableViewCellStyle.Value1);
            var labelElement = new StringElement("Labels", string.Empty, UITableViewCellStyle.Value1);
            var assigneeElement = new StringElement("Assignee", string.Empty, UITableViewCellStyle.Value1);
            var milestoneElement = new StringElement("Milestone", string.Empty, UITableViewCellStyle.Value1);
            var fieldElement = new StringElement("Field", string.Empty, UITableViewCellStyle.Value1);
            var ascElement = new BooleanElement("Ascending", false);

            var filterSection = new Section("Filter") { stateElement, mentionedElement, creatorElement, labelElement, assigneeElement, milestoneElement };
            var orderSection = new Section("Order By") { fieldElement, ascElement };
            var searchSection = new Section();
            var footerButton = new TableFooterButton("Search!");
            searchSection.FooterView = footerButton;
            source.Root.Add(filterSection, orderSection, searchSection);

            OnActivation(d => {
                d(assigneeElement.Clicked.InvokeCommand(ViewModel.SelectAssigneeCommand));
                d(milestoneElement.Clicked.InvokeCommand(ViewModel.SelectMilestoneCommand));
                d(fieldElement.Clicked.InvokeCommand(ViewModel.SelectSortCommand));
                d(stateElement.Clicked.InvokeCommand(ViewModel.SelectStateCommand));
                d(labelElement.Clicked.InvokeCommand(ViewModel.SelectLabelsCommand));
                d(footerButton.Clicked.InvokeCommand(ViewModel.SaveCommand));
                d(ascElement.Changed.Subscribe(x => ViewModel.Ascending = x));
                d(mentionedElement.Changed.Subscribe(x => ViewModel.Mentioned = x));
                d(creatorElement.Changed.Subscribe(x => ViewModel.Creator = x));

                d(this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .ToBarButtonItem(Images.Cancel, x => NavigationItem.LeftBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                    .ToBarButtonItem(Images.Search, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.State).Subscribe(x => stateElement.Value = x.Humanize()));
                d(this.WhenAnyValue(x => x.ViewModel.LabelsString).Subscribe(x => labelElement.Value = x));
                d(this.WhenAnyValue(x => x.ViewModel.Mentioned).Subscribe(x => mentionedElement.Value = x));
                d(this.WhenAnyValue(x => x.ViewModel.Creator).Subscribe(x => creatorElement.Value = x));
                d(this.WhenAnyValue(x => x.ViewModel.AssigneeString).Subscribe(x => assigneeElement.Value = x));
                d(this.WhenAnyValue(x => x.ViewModel.MilestoneString).Subscribe(x => milestoneElement.Value = x));
                d(this.WhenAnyValue(x => x.ViewModel.SortType).Subscribe(x => fieldElement.Value = x.Humanize()));
                d(this.WhenAnyValue(x => x.ViewModel.Ascending).Subscribe(x => ascElement.Value = x));

                d(this.WhenAnyObservable(x => x.ViewModel.SelectAssigneeCommand)
                    .Subscribe(_ => IssueAssigneeViewController.Show(this, ViewModel.CreateAssigneeViewModel())));

                d(this.WhenAnyObservable(x => x.ViewModel.SelectMilestoneCommand)
                    .Subscribe(_ => IssueMilestonesViewController.Show(this, ViewModel.CreateMilestonesViewModel())));

                d(this.WhenAnyObservable(x => x.ViewModel.SelectLabelsCommand)
                    .Subscribe(_ => IssueLabelsViewController.Show(this, ViewModel.CreateLabelsViewModel())));
            });
        }
    }
}

