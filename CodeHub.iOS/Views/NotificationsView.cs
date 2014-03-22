using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views
{
    public class NotificationsView : ViewModelCollectionDrivenDialogViewController
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;
        private IHud _markHud;

        public NotificationsView()
        {
            _viewSegment = new UISegmentedControl(new object[] { "Unread".t(), "Participating".t(), "All".t() });
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
        }

        public override void ViewDidLoad()
        {
            NoItemsText = "No Notifications".t();
            Title = "Notifications".t();

            base.ViewDidLoad();

            _markHud = new Hud(View);
            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

            var vm = (NotificationsViewModel)ViewModel;
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.CheckButton, UIBarButtonItemStyle.Plain, (s, e) => vm.ReadAllCommand.Execute(null));
            vm.ReadAllCommand.CanExecuteChanged += (sender, e) => NavigationItem.RightBarButtonItem.Enabled = vm.ReadAllCommand.CanExecute(null);

            vm.Bind(x => x.IsMarking, x =>
            {
                if (x)
                    _markHud.Show("Marking...");
                else
                    _markHud.Hide();
            });

            BindCollection(vm.Notifications, x =>
            {
                var el = new StyledStringElement(x.Subject.Title, x.UpdatedAt.ToDaysAgo(), UITableViewCellStyle.Subtitle) { Accessory = UITableViewCellAccessory.DisclosureIndicator };

                var subject = x.Subject.Type.ToLower();
                if (subject.Equals("issue"))
                    el.Image = Images.Flag;
                else if (subject.Equals("pullrequest"))
                    el.Image = Images.Hand;
                else if (subject.Equals("commit"))
                    el.Image = Images.Commit;
                else if (subject.Equals("release"))
                    el.Image = Images.Tag;
                else
                    el.Image = Images.Notifications;

                el.Tapped += () => vm.GoToNotificationCommand.Execute(x);
                return el;
            });

            var set = this.CreateBindingSet<NotificationsView, NotificationsViewModel>();
            set.Bind(_viewSegment).To(x => x.ShownIndex);
            set.Apply();
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
                : base(new System.Drawing.RectangleF(0, 0, 320, 28f))
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
                    _button.Frame = new System.Drawing.RectangleF(Frame.Width - 42f, 1, 26, 26);
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

