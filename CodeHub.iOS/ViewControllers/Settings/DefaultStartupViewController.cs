using System;
using UIKit;
using System.Linq;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using Splat;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers.Settings
{
    public class DefaultStartupViewController : TableViewController
    {
        private readonly IApplicationService _applicationService;
        private readonly Action _doneAction;

        private static readonly string[] _items = {
            "News",
            "Organizations",
            "Trending Repositories",
            "Explore Repositories",
            "Owned Repositories",
            "Starred Repositories",
            "Public Gists",
            "Starred Gists",
            "My Gists",
            "Profile",
            "My Events",
            "My Issues",
            "Notifications"
        };

        private string _selectedValue;
        private string SelectedValue
        {
            get { return _selectedValue; }
            set { this.RaiseAndSetIfChanged(ref _selectedValue, value); }
        }

        public DefaultStartupViewController(
            Action doneAction,
            IApplicationService applicationService = null)
            : base(UITableViewStyle.Plain)
        {
            _doneAction = doneAction;
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            Title = "Default Startup View";
            SelectedValue = _applicationService.Account.DefaultStartupView;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DialogTableViewSource(TableView);
            TableView.Source = source;

            this.WhenActivated(d =>
            {
                d(this.WhenAnyValue(x => x.SelectedValue)
                  .Subscribe(_ => Render(source)));
            });
        }

        private void Render(DialogTableViewSource tableViewSource)
        {
            var section = new Section();
            section.Add(_items.Select(CreateElement));
            tableViewSource.Root.Reset(section);
        }

        private Element CreateElement(string title)
        {
            var element = new StringElement(title);
            element.Clicked.Select(_ => title).Subscribe(ElementSelected);
            element.Accessory = string.Equals(title, SelectedValue)
                ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            return element;
        }

        private void ElementSelected(string value)
        {
            SelectedValue = value;
            _applicationService.Account.DefaultStartupView = value;
            _applicationService.UpdateActiveAccount().ToBackground();
            _doneAction();
        }
    }
}

