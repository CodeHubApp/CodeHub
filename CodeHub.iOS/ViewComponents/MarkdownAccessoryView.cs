using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using ReactiveUI;

namespace CodeHub.iOS.ViewComponents
{
    public class MarkdownAccessoryView : ButtonAccessoryView
    {
        public MarkdownAccessoryView(UITextView controller, IReactiveCommand<string> postImage)
            : base(CreateButtons(controller, postImage))
        {
        }

        private static IEnumerable<UIButton> CreateButtons(UITextView controller, IReactiveCommand<string> postImage)
        {
            var pictureImage = UIImageHelper.FromFileAuto("Images/MarkdownComposer/picture");
            var linkImage = UIImageHelper.FromFileAuto("Images/MarkdownComposer/link");
            var photoImage = UIImageHelper.FromFileAuto("Images/MarkdownComposer/photo");

            return new []
            {
                CreateAccessoryButton("@", () => controller.InsertText("@")),
                CreateAccessoryButton("#", () => controller.InsertText("#")),
                CreateAccessoryButton("*", () => controller.InsertText("*")),
                CreateAccessoryButton("`", () => controller.InsertText("`")),
                CreateAccessoryButton(pictureImage, () => {
                    var range = controller.SelectedRange;
                    controller.InsertText("![]()");
                    controller.SelectedRange = new MonoTouch.Foundation.NSRange(range.Location + 4, 0);
                }),

                CreateAccessoryButton(photoImage, () => 
                    postImage.ExecuteAsync().Catch(Observable.Empty<string>())
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => controller.InsertText("![Image](" + x + ")"))),

                CreateAccessoryButton(linkImage, () => {
                    var range = controller.SelectedRange;
                    controller.InsertText("[]()");
                    controller.SelectedRange = new MonoTouch.Foundation.NSRange(range.Location + 1, 0);
                }),
                CreateAccessoryButton("~", () => controller.InsertText("~")),
                CreateAccessoryButton("=", () => controller.InsertText("=")),
                CreateAccessoryButton("-", () => controller.InsertText("-")),
                CreateAccessoryButton("+", () => controller.InsertText("+")),
                CreateAccessoryButton("_", () => controller.InsertText("_")),
                CreateAccessoryButton("[", () => controller.InsertText("[")),
                CreateAccessoryButton("]", () => controller.InsertText("]")),
                CreateAccessoryButton("<", () => controller.InsertText("<")),
                CreateAccessoryButton(">", () => controller.InsertText(">")),
            };
        }
    }
}

