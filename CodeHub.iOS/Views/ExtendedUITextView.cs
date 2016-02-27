using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.Views
{
    public class ExtendedUITextView : UITextView
    {
        private readonly UILabel _placeholderView = new UILabel();

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                _placeholderView.Hidden = Text.Length > 0;
            }
        }

        public override UIFont Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                _placeholderView.Font = value;
            }
        }

        public string Placeholder
        {
            get { return _placeholderView.Text; }
            set { _placeholderView.Text = value; }
        }

        public ExtendedUITextView()
        {
            _placeholderView.TextColor = UIColor.FromWhiteAlpha(0.702f, 1.0f);
            _placeholderView.Frame = new CGRect(5, 8, 300f, 20f);
            _placeholderView.UserInteractionEnabled = false;
            _placeholderView.Font = UIFont.PreferredBody;
            _placeholderView.Hidden = Text.Length > 0;
            this.Add(_placeholderView);

            this.Changed += (sender, e) =>
                _placeholderView.Hidden = !string.IsNullOrEmpty(Text);
        }
    }
}

