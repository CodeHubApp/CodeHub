using System;
using CodeHub.iOS.ViewControllers;
using UIKit;
using CodeHub.iOS.Utilities;
using CodeHub.Core.ViewModels.Notifications;
using Humanizer;
using MvvmCross.Binding.BindingContext;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views
{
    public class NotificationsView : ViewModelCollectionDrivenDialogViewController
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;

        public NotificationsView()
        {
            _viewSegment = new UISegmentedControl(new object[] { "Unread", "Participating", "All" });
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
        }

        public override void ViewDidLoad()
        {
            NoItemsText = "No Notifications";
            Title = "Notifications";

            base.ViewDidLoad();

            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

            var checkButton = new UIBarButtonItem { Image = Theme.CurrentTheme.CheckButton };
            NavigationItem.RightBarButtonItem = checkButton;

            var vm = (NotificationsViewModel)ViewModel;
            var weakVm = new WeakReference<NotificationsViewModel>(vm);

            vm.ReadAllCommand.CanExecuteChanged += (sender, e) => NavigationItem.RightBarButtonItem.Enabled = vm.ReadAllCommand.CanExecute(null);

            vm.Bind(x => x.IsMarking).SubscribeStatus("Marking...");

            BindCollection(vm.Notifications, x =>
            {
                var el = new StringElement(x.Subject.Title, x.UpdatedAt.Humanize(), UITableViewCellStyle.Subtitle) { Accessory = UITableViewCellAccessory.DisclosureIndicator };

                var subject = x.Subject.Type.ToLower();
                if (subject.Equals("issue"))
                    el.Image = Octicon.IssueOpened.ToImage();
                else if (subject.Equals("pullrequest"))
                    el.Image = Octicon.GitPullRequest.ToImage();
                else if (subject.Equals("commit"))
                    el.Image = Octicon.GitCommit.ToImage();
                else if (subject.Equals("release"))
                    el.Image = Octicon.Tag.ToImage();
                else
                    el.Image = Octicon.Alert.ToImage();

                el.Clicked.Subscribe(_ => weakVm.Get()?.GoToNotificationCommand.Execute(x));
                return el;
            });

            var set = this.CreateBindingSet<NotificationsView, NotificationsViewModel>();
            set.Bind(_viewSegment).To(x => x.ShownIndex);
            set.Apply();

            OnActivation(d =>
            {
                d(checkButton.GetClickedObservable().BindCommand(vm.ReadAllCommand));
            });
        }

        protected override Section CreateSection(string text)
        {
            return new Section(new MarkReadSection(text, this, _viewSegment.SelectedSegment != 2));
        }

        private class MarkReadSection : UITableViewHeaderFooterView
        {
            readonly UIButton _button;
            readonly NotificationsView _parent;
            public MarkReadSection(string text, NotificationsView parent, bool button)
                : base(new CoreGraphics.CGRect(0, 0, 320, 28f))
            {
                _parent = parent;
                TextLabel.Text = text;
                TextLabel.Font = TextLabel.Font.WithSize(TextLabel.Font.PointSize * Theme.CurrentTheme.FontSizeRatio);

                if (button)
                {
                    _button = new UIButton(UIButtonType.RoundedRect);
                    _button.SetImage(Theme.CurrentTheme.CheckButton, UIControlState.Normal);
                    //_button.Frame = new System.Drawing.RectangleF(320f - 42f, 1f, 26f, 26f);
                    _button.TintColor = UIColor.FromRGB(50, 50, 50);
                    _button.TouchUpInside += (sender, e) => ((NotificationsViewModel)_parent.ViewModel).ReadRepositoriesCommand.Execute(text);
                    Add(_button);
                }
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                if (_button != null)
                    _button.Frame = new CoreGraphics.CGRect(Frame.Width - 42f, 1, 26, 26);
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
            base.ViewWillDisappear(animated);
        }
    }
}

