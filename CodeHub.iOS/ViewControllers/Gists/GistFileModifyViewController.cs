using System;
using CodeHub.iOS.DialogElements;
using ReactiveUI;
using UIKit;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.iOS.Services;
using System.Threading.Tasks;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class GistFileEditViewController :  GistFileModifyViewController
    {
    }

    public class GistFileAddViewController : GistFileModifyViewController
    {
    }

    public abstract class GistFileModifyViewController : DialogViewController
    {
        public Action<string, string> Save;

        private string _filename;
        public string Filename
        {
            get { return _filename; }
            set { this.RaiseAndSetIfChanged(ref _filename, value); }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
        }

        public IReactiveCommand<Unit> SaveCommand { get; }

        protected GistFileModifyViewController()
            : base(UITableViewStyle.Plain)
        {
            SaveCommand = ReactiveCommand.CreateAsyncTask(t => {
                if (String.IsNullOrEmpty(Content))
                    throw new Exception("You cannot save a file without content!");
                Save?.Invoke(Filename, Content);
                return Task.FromResult(Unit.Default);
            });

            SaveCommand.ThrownExceptions.Subscribe(x => AlertDialogService.ShowAlert("Error", x.Message));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
   
            var titleElement = new DummyInputElement("Title") { SpellChecking = false };
            var contentElement = new ExpandingInputElement("Content");

            Root.Add(new Section { titleElement, contentElement });
            TableView.TableFooterView = new UIView();

            OnActivation(d => {
                d(this.Bind(x => x.Filename, true).Subscribe(x => {
                    Title = string.IsNullOrEmpty(x) ? "Gist File" : x;
                    titleElement.Value = x;
                }));
                d(titleElement.Changed.Subscribe(x => Filename = x));

                d(this.Bind(x => x.Content, true).Subscribe(x => contentElement.Value = x));
                d(contentElement.Changed.Subscribe(x => Content = x));

                d(SaveCommand.Subscribe(_ => ResignFirstResponder()));
                d(this.Bind(x => x.SaveCommand, true).ToBarButtonItem(Images.Buttons.SaveButton, x => NavigationItem.RightBarButtonItem = x));
            });
        }
    }
}

