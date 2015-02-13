using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using UIKit;
using CodeHub.iOS.ViewControllers;

namespace CodeHub.iOS.Views.Issues
{
    public abstract class IssueModifyView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : IssueModifyViewModel
    {
        private readonly DummyInputElement _titleElement = new DummyInputElement("Name");
        private readonly ExpandingInputElement _descriptionElement = new ExpandingInputElement("Description");
        private readonly StringElement _milestoneElement;
        private readonly StringElement _assigneeElement;
        private readonly StringElement _labelsElement;

        protected IssueModifyView()
        {
            this.WhenAnyValue(x => x.ViewModel.GoToAssigneesCommand)
                .Switch()
                .Subscribe(_ => ShowAssigneeSelector());

            this.WhenAnyValue(x => x.ViewModel.GoToMilestonesCommand)
                .Switch()
                .Subscribe(_ => ShowMilestonesSelector());

            this.WhenAnyValue(x => x.ViewModel.GoToLabelsCommand)
                .Switch()
                .Subscribe(_ => ShowLabelsSelector());

            this.WhenViewModel(x => x.Subject).Subscribe(x => _titleElement.Value = x);
            _titleElement.Changed += (sender, e) => ViewModel.Subject = _titleElement.Value;

            this.WhenViewModel(x => x.Content).Subscribe(x => _descriptionElement.Value = x);
            _descriptionElement.ValueChanged += (sender, e) => ViewModel.Content = _descriptionElement.Value;

            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Save))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            _milestoneElement = new StringElement("Milestone", string.Empty, UITableViewCellStyle.Value1);
            _milestoneElement.Tapped = () => ViewModel.GoToMilestonesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedMilestone)
                .Select(x => x == null ? "No Milestone" : x.Title)
                .Subscribe(x => _milestoneElement.Value = x);

            _assigneeElement = new StringElement("Assigned", string.Empty, UITableViewCellStyle.Value1);
            _assigneeElement.Tapped = () => ViewModel.GoToAssigneesCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedUser)
                .Select(x => x == null ? "Unassigned" : x.Login)
                .Subscribe(x => _assigneeElement.Value = x);

            _labelsElement = new StringElement("Labels", string.Empty, UITableViewCellStyle.Value1);
            _labelsElement.Tapped = () => ViewModel.GoToLabelsCommand.ExecuteIfCan();
            this.WhenAnyValue(x => x.ViewModel.AssignedLabels)
                .Select(x => (x == null || x.Count == 0) ? "None" : string.Join(",", x.Select(y => y.Name)))
                .Subscribe(x => _labelsElement.Value = x);
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


        private void ShowAssigneeSelector()
        {
            var viewController = new IssueAssigneeView { Title = "Assignees" };
            viewController.ViewModel = ViewModel.Assignees;
            viewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, (s, e) => viewController.DismissViewController(true, null));
            PresentViewController(new ThemedNavigationController(viewController), true, null);
        }

        private void ShowLabelsSelector()
        {
            var viewController = new IssueLabelsView { Title = "Labels" };
            viewController.ViewModel = ViewModel.Labels;
            viewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, (s, e) => {
                ViewModel.Labels.SelectLabelsCommand.ExecuteIfCan();
                viewController.DismissViewController(true, null);
            });
            PresentViewController(new ThemedNavigationController(viewController), true, null);
        }

        private void ShowMilestonesSelector()
        {
            var viewController = new IssueMilestonesView { Title = "Milestones" };
            viewController.ViewModel = ViewModel.Milestones;
            viewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, (s, e) => viewController.DismissViewController(true, null));
            PresentViewController(new ThemedNavigationController(viewController), true, null);
        }
    }
}

