using System;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewComponents;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Issues
{
    public class IssuesView : BaseTableViewController<IssuesViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new [] { "Open", "Closed", "Mine" });

        public IssuesView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.IssueOpened.ToImage(64f), "There are no issues."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var filterBarButtonItem = new UIBarButtonItem(Images.Filter, UIBarButtonItemStyle.Plain, 
                (s, e) => ViewModel.GoToCustomFilterCommand.ExecuteIfCan());

            this.WhenAnyValue(x => x.ViewModel.CustomFilterEnabled)
                .Subscribe(x => filterBarButtonItem.Image = x ? Images.FilterFilled : Images.Filter);

            this.WhenAnyValue(x => x.ViewModel.GoToNewIssueCommand, x => x.ViewModel.GoToCustomFilterCommand)
                .Select(x => new [] { x.Item1.ToBarButtonItem(UIBarButtonSystemItem.Add), filterBarButtonItem })
                .Subscribe(x => NavigationItem.RightBarButtonItems = x);

            this.WhenAnyValue(x => x.ViewModel.CustomFilterEnabled)
                .Where(x => x)
                .Subscribe(x => _viewSegment.SelectedSegment = -1);

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
    }
}

