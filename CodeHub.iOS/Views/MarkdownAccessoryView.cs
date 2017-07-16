using UIKit;
using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using ReactiveUI;
using CodeHub.Core.ViewModels;

namespace CodeHub.iOS.Views
{
    public class MarkdownAccessoryView : ButtonAccessoryView
    {
        public MarkdownAccessoryView(UITextView controller)
            : base(CreateButtons(controller))
        {
        }

        private static IEnumerable<UIButton> CreateButtons(UITextView controller)
        {
            var pictureImage = Octicon.FileMedia.ToImage(64);
            var linkImage = Octicon.Link.ToImage(64);
            var photoImage = Octicon.DeviceCamera.ToImage(64);

            var vm = new MarkdownAccessoryViewModel();

            return new []
            {
                CreateAccessoryButton("@", () => controller.InsertText("@")),
                CreateAccessoryButton("#", () => controller.InsertText("#")),
                CreateAccessoryButton("*", () => controller.InsertText("*")),
                CreateAccessoryButton("`", () => controller.InsertText("`")),
                CreateAccessoryButton(pictureImage, () => {
                    var range = controller.SelectedRange;
                    controller.InsertText("![]()");
                    controller.SelectedRange = new Foundation.NSRange(range.Location + 4, 0);
                }),

                CreateAccessoryButton(photoImage, () => 
                  vm.PostToImgurCommand.Execute()
	                .Catch(Observable.Empty<string>())
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => controller.InsertText("![Image](" + x + ")"))),

                CreateAccessoryButton(linkImage, () => {
                    var range = controller.SelectedRange;
                    controller.InsertText("[]()");
                    controller.SelectedRange = new Foundation.NSRange(range.Location + 1, 0);
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

