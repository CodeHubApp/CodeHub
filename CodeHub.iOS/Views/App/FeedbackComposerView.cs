using System;
using Xamarin.Utilities.ViewControllers;
using CodeHub.Core.ViewModels.App;
using Xamarin.Utilities.DialogElements;
using System.Reactive.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using CodeHub.iOS.ViewComponents;
using ReactiveUI;
using Xamarin.Utilities.Delegates;

namespace CodeHub.iOS.Views.App
{
    public class FeedbackComposerView : ReactiveTableViewController<FeedbackComposerViewModel>
    {
        private readonly InputElement _titleElement = new CustomTextElement("Title");
        private readonly CustomInputElement _descriptionElement;

        public FeedbackComposerView()
        {
            _descriptionElement = new CustomInputElement(() => ViewModel.PostToImgurCommand, "Description");

            this.WhenViewModel(x => x.Subject).Subscribe(x => _titleElement.Value = x);
            _titleElement.Changed += (sender, e) => ViewModel.Subject = _titleElement.Value;

            this.WhenViewModel(x => x.Description).Subscribe(x => _descriptionElement.Value = x);
            _descriptionElement.ValueChanged += (sender, e) => ViewModel.Description = _descriptionElement.Value;

            this.WhenViewModel(x => x.SubmitCommand).Subscribe(x =>
            {
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, (s, e) =>
                {
                    ResignFirstResponder();
                    x.ExecuteIfCan();
                });

                x.CanExecuteObservable.Subscribe(y => NavigationItem.RightBarButtonItem.Enabled = y);
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DialogTableViewSource(TableView, true);
            source.Root.Add(new Section { _titleElement, _descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();
        }

        private class CustomTextElement : InputElement
        {
            public CustomTextElement(string name)
                : base(name, name, string.Empty)
            {
            }

            protected override UITextField CreateTextField(RectangleF frame)
            {
                var txt = base.CreateTextField(frame);
                txt.AllEditingEvents += (sender, e) => FetchValue();
                return txt;
            }
        }

        public class CustomInputElement : Element, IElementSizing
        {
            public string Value
            {
                get { return _val; }
                set
                {
                    if (string.Equals(_val, value))
                        return;

                    _val = value;
                    if (ValueChanged != null)
                        ValueChanged(this, EventArgs.Empty);
                }
            }
            private string _val;
            public event EventHandler ValueChanged;
            private readonly Func<IReactiveCommand<string>> _postImageFunc;
            private readonly string _description;

            public CustomInputElement(Func<IReactiveCommand<string>> postImageFunc, string description)
            {
                _postImageFunc = postImageFunc;
                _description = description;
            }

            public override UITableViewCell GetCell(UITableView tv)
            {
                var cell = tv.DequeueReusableCell(CustomInputCell.Key) as CustomInputCell;
                if (cell == null)
                {
                    cell = new CustomInputCell(_postImageFunc(), _description);
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cell.TextView.Ended += delegate {
                        Value = cell.TextView.Text;
                    };

                    cell.TextView.Changed += (s, e) => {
                        Value = cell.TextView.Text;
                 
                        tv.BeginUpdates();
                        tv.EndUpdates();

                        var caret = cell.TextView.GetCaretRectForPosition(cell.TextView.SelectedTextRange.Start);
                        var cursorRect = tv.ConvertRectFromView(caret, cell.TextView);
                        var kk = cursorRect.Size;
                        kk.Height += 8.0f;
                        cursorRect.Size = kk;
                        tv.ScrollRectToVisible(cursorRect, false);
                    };

                    //cell.TextView.ReturnKeyType = UIReturnKeyType.Done;
                }

                cell.TextView.Text = Value ?? string.Empty;
                return cell;
            }

            private class CustomInputCell : UITableViewCell
            {
                public static NSString Key = new NSString("CustomInputCell");
                public static UIFont InputFont = UIFont.SystemFontOfSize(14f);
                public readonly UITextView TextView;

                public CustomInputCell(IReactiveCommand<string> postImageFunc, string placeholder)
                    : base(UITableViewCellStyle.Default, Key)
                {
                    TextView = new CustomTextView(placeholder)
                    { 
                        Frame = new RectangleF(12, 0, ContentView.Frame.Width - 24f, ContentView.Frame.Height),
                        ScrollEnabled = false,
                    };
                    TextView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                    TextView.BackgroundColor = UIColor.Clear;
                    TextView.Font = InputFont;
                    TextView.InputAccessoryView = new MarkdownAccessoryView(TextView, postImageFunc) { Frame = new RectangleF(0, 0, 320f, 44f) };
                    ContentView.Add(TextView);
                }

                public override void LayoutSubviews()
                {
                    base.LayoutSubviews();
                    SeparatorInset = new UIEdgeInsets(0, Bounds.Width, 0, 0);
                }

                private class CustomTextView : UITextView
                {
                    private readonly UILabel _placeholderView = new UILabel();
                    public string Placeholder { get; set; }

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

                    public CustomTextView(string placeholder)
                    {
                        _placeholderView.Text = placeholder;
                        _placeholderView.TextColor = UIColor.FromWhiteAlpha(0.702f, 1.0f);
                        _placeholderView.Frame = new RectangleF(5, 8, 100f, 16f);
                        _placeholderView.UserInteractionEnabled = false;
                        this.Add(_placeholderView);

                        this.Changed += (sender, e) =>
                            _placeholderView.Hidden = !string.IsNullOrEmpty(Text);
                    }
                }
            }

            public float GetHeight(UITableView tableView, NSIndexPath indexPath)
            {
                var str = new NSString(Value ?? string.Empty);
                var height = (int)str.StringSize(CustomInputCell.InputFont, new SizeF(tableView.Bounds.Width - 24f, 10000), UILineBreakMode.WordWrap).Height + 60f;
                return height > 60 ? height : 60;
            }
        }
    }
}
