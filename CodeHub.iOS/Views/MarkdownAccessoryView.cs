using UIKit;
using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using ReactiveUI;
using Splat;
using CodeHub.Core.Services;
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
            var pictureImage = UIImageHelper.FromFileAuto("Images/MarkdownComposer/picture");
            var linkImage = UIImageHelper.FromFileAuto("Images/MarkdownComposer/link");
            var photoImage = UIImageHelper.FromFileAuto("Images/MarkdownComposer/photo");

            var serviceConstructor = Locator.Current.GetService<IServiceConstructor>();
            var vm = serviceConstructor.Construct<MarkdownAccessoryViewModel>();

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
                    vm.PostToImgurCommand.ExecuteAsync().Catch(Observable.Empty<string>())
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

