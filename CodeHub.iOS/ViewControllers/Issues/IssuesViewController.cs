using System;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Views;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using CodeHub.iOS.ViewControllers;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class IssuesViewController : BaseTableViewController<IssuesViewModel>
    {
        private UISegmentedControl _viewSegment;

        public IssuesViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.IssueOpened.ToImage(64f), "There are no issues."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _viewSegment = new UISegmentedControl(new [] { "Open", "Closed", "Mine" });

            var filterBarButtonItem = new UIBarButtonItem(Images.Filter, UIBarButtonItemStyle.Plain, 
                (s, e) => ViewModel.GoToCustomFilterCommand.ExecuteIfCan());

            this.WhenAnyValue(x => x.ViewModel.GoToNewIssueCommand, x => x.ViewModel.GoToCustomFilterCommand)
                .Select(x => new [] { x.Item1.ToBarButtonItem(UIBarButtonSystemItem.Add), filterBarButtonItem })
                .Subscribe(x => NavigationItem.RightBarButtonItems = x);

            this.WhenAnyValue(x => x.ViewModel.FilterSelection)
                .Select(x => x == IssuesViewModel.IssueFilterSelection.Custom)
                .Subscribe(x => {
                    filterBarButtonItem.Image = x ? Images.FilterFilled : Images.Filter;
                    if (x) _viewSegment.SelectedSegment = -1;
            });

            this.WhenAnyObservable(x => x.ViewModel.GoToCustomFilterCommand)
                .Subscribe(_ => ShowFilterView());

            NavigationItem.TitleView = _viewSegment;
            TableView.Source = new IssueTableViewSource(TableView, ViewModel.Issues);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;
            _viewSegment.SelectedSegment = (int)ViewModel.FilterSelection;
            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            switch (_viewSegment.SelectedSegment)
            {
                case 0:
                    ViewModel.FilterSelection = IssuesViewModel.IssueFilterSelection.Open;
                    break;
                case 1:
                    ViewModel.FilterSelection = IssuesViewModel.IssueFilterSelection.Closed;
                    break;
                case 2:
                    ViewModel.FilterSelection = IssuesViewModel.IssueFilterSelection.Mine;
                    break;
            }
        }

        private void ShowFilterView()
        {
            var controller = new RepositoryIssuesFilterViewController { ViewModel = ViewModel.Filter };
            var navigation = new ThemedNavigationController(controller);
            PresentViewController(navigation, true, null);
        }
    }
}

