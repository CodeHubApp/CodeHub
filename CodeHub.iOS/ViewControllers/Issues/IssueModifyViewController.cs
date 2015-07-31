using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using UIKit;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public abstract class IssueModifyViewController<TViewModel> : BaseTableViewController<TViewModel>, IModalViewController 
        where TViewModel : IssueModifyViewModel
    {
        private readonly DummyInputElement _titleElement = new DummyInputElement("Name");
        private readonly ExpandingInputElement _descriptionElement = new ExpandingInputElement("Description");
        private readonly StringElement _milestoneElement;
        private readonly StringElement _assigneeElement;
        private readonly StringElement _labelsElement;

        protected IssueModifyViewController()
        {
            _descriptionElement.AccessoryView = x => new MarkdownAccessoryView(x);

            this.WhenAnyObservable(x => x.ViewModel.GoToAssigneesCommand)
                .Subscribe(_ => IssueAssigneeViewController.Show(this, ViewModel.CreateAssigneeViewModel()));

            this.WhenAnyObservable(x => x.ViewModel.GoToMilestonesCommand)
                .Subscribe(_ => IssueMilestonesViewController.Show(this, ViewModel.CreateMilestonesViewModel()));

            this.WhenAnyObservable(x => x.ViewModel.GoToLabelsCommand)
                .Subscribe(_ => IssueLabelsViewController.Show(this, ViewModel.CreateLabelsViewModel()));

            this.WhenAnyValue(x => x.ViewModel.Subject).Subscribe(x => _titleElement.Value = x);
            _titleElement.Changed += (sender, e) => ViewModel.Subject = _titleElement.Value;

            this.WhenAnyValue(x => x.ViewModel.Content).Subscribe(x => _descriptionElement.Value = x);
            _descriptionElement.ValueChanged += (sender, e) => ViewModel.Content = _descriptionElement.Value;

            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Save))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            _milestoneElement = new StringElement("Milestone", string.Empty, UITableViewCellStyle.Value1);
            _milestoneElement.Tapped = () => ViewModel.GoToMilestonesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.Milestone)
                .Select(x => x == null ? "No Milestone" : x.Title)
                .Subscribe(x => _milestoneElement.Value = x);

            _assigneeElement = new StringElement("Assigned", string.Empty, UITableViewCellStyle.Value1);
            _assigneeElement.Tapped = () => ViewModel.GoToAssigneesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.Assignee)
                .Select(x => x == null ? "Unassigned" : x.Login)
                .Subscribe(x => _assigneeElement.Value = x);

            _labelsElement = new StringElement("Labels", string.Empty, UITableViewCellStyle.Value1);
            _labelsElement.Tapped = () => ViewModel.GoToLabelsCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.Labels)
                .Select(x => (x == null || x.Count == 0) ? "None" : string.Join(",", x.Select(y => y.Name)))
                .Subscribe(x => _labelsElement.Value = x);

            this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                .Select(x => x.ToBarButtonItem(Images.Cancel))
                .Subscribe(x => NavigationItem.LeftBarButtonItem = x);
        }

        protected override void LoadViewModel()
        {
            // Dont' delete.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var section = new Section();
            var source = new DialogTableViewSource(TableView);
            TableView.Source = source;
            TableView.TableFooterView = new UIView();

            source.Root.Add(new Section { _titleElement }, section, new Section { _descriptionElement });

            this.WhenAnyValue(x => x.ViewModel.IsCollaborator)
                .Where(x => x.HasValue && x.Value)
                .Where(_ => _milestoneElement.Section == null)
                .Subscribe(x => section.Add(new [] { _assigneeElement, _milestoneElement, _labelsElement }));

            this.WhenAnyValue(x => x.ViewModel.IsCollaborator)
                .Where(x => !x.HasValue || !x.Value)
                .Subscribe(x => section.Clear());
        }
    }
}

