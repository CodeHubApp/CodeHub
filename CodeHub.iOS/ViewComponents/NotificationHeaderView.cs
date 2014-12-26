using CodeHub.Core.ViewModels.Notifications;
using MonoTouch.UIKit;
using ReactiveUI;
using System;

namespace CodeHub.iOS.ViewComponents
{
    public class NotificationHeaderView : UITableViewHeaderFooterView
    {
        readonly UIButton _button;

        public NotificationHeaderView(NotificationGroupViewModel viewModel)
            : base(new System.Drawing.RectangleF(0, 0, 320, 30f))
        {
            TextLabel.Text = viewModel.Name;

            if (viewModel.ReadAllCommand != null)
            {
                _button = new UIButton(UIButtonType.RoundedRect);
                _button.SetImage(Images.CheckButton, UIControlState.Normal);
                //_button.Frame = new System.Drawing.RectangleF(320f - 42f, 1f, 26f, 26f);
                _button.TintColor = UIColor.FromRGB(50, 50, 50);
                _button.TouchUpInside += (sender, e) => viewModel.ReadAllCommand.ExecuteIfCan();
                Add(_button);
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (_button != null)
            {
                _button.Frame = new System.Drawing.RectangleF(Frame.Width - 42f, 1, 26, 26);
            }
        }
    }

}