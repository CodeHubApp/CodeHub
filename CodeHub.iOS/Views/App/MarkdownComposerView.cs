using System;
using System.Linq;
using CodeHub.Core.ViewModels.App;
using MonoTouch.UIKit;
using Xamarin.Utilities.ViewControllers;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.iOS.WebViews;
using Splat;
using Xamarin.Utilities.Factories;

namespace CodeHub.iOS.Views.App
{
    public abstract class MarkdownComposerView<TViewModel> : ComposerViewController<TViewModel> where TViewModel : MarkdownComposerViewModel
    {
        private readonly IAlertDialogFactory _alertDialogService;
        private readonly UISegmentedControl _viewSegment;
        private UIWebView _previewView;

        protected MarkdownComposerView(IAlertDialogFactory alertDialogService)
        {
            _alertDialogService = alertDialogService;
            _viewSegment = new UISegmentedControl(new [] { "Compose", "Preview" });
            _viewSegment.SelectedSegment = 0;
            NavigationItem.TitleView = _viewSegment;
            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var pictureImage = UIImageHelper.FromFileAuto("Images/MarkdownComposer/picture");
            var linkImage = UIImageHelper.FromFileAuto("Images/MarkdownComposer/link");
            var photoImage = UIImageHelper.FromFileAuto("Images/MarkdownComposer/photo");

            var buttons = new []
            {
                CreateAccessoryButton("@", () => TextView.InsertText("@")),
                CreateAccessoryButton("#", () => TextView.InsertText("#")),
                CreateAccessoryButton("*", () => TextView.InsertText("*")),
                CreateAccessoryButton("`", () => TextView.InsertText("`")),
                CreateAccessoryButton(pictureImage, () => {
                    var range = TextView.SelectedRange;
                    TextView.InsertText("![]()");
                    TextView.SelectedRange = new MonoTouch.Foundation.NSRange(range.Location + 4, 0);
                }),
                CreateAccessoryButton(photoImage, SelectImage),
                CreateAccessoryButton(linkImage, () => {
                    var range = TextView.SelectedRange;
                    TextView.InsertText("[]()");
                    TextView.SelectedRange = new MonoTouch.Foundation.NSRange(range.Location + 1, 0);
                }),
                CreateAccessoryButton("~", () => TextView.InsertText("~")),
                CreateAccessoryButton("=", () => TextView.InsertText("=")),
                CreateAccessoryButton("-", () => TextView.InsertText("-")),
                CreateAccessoryButton("+", () => TextView.InsertText("+")),
                CreateAccessoryButton("_", () => TextView.InsertText("_")),
                CreateAccessoryButton("[", () => TextView.InsertText("[")),
                CreateAccessoryButton("]", () => TextView.InsertText("]")),
                CreateAccessoryButton("<", () => TextView.InsertText("<")),
                CreateAccessoryButton(">", () => TextView.InsertText(">")),
            };

            SetAccesoryButtons(buttons);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem("Save", UIBarButtonItemStyle.Done, (s, e) => ViewModel.SaveCommand.ExecuteIfCan());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.SaveCommand);
        }

        private class ImagePickerDelegate : UINavigationControllerDelegate
        {
            public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
            {
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            }
        }

       
        private async void UploadImage(UIImage img)
        {
//            if (!ViewModel.PostToImgurCommand.CanExecute(null))
//                return;
//
//            try
//            {
//                var data = img.AsJPEG();
//                var dataBytes = new byte[data.Length];
//                System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));
//                var imgurModel = await ViewModel.PostToImgurCommand.ExecuteAsync(dataBytes);
//                TextView.InsertText("![Image](" + imgurModel.Data.Link + ")");
//            }
//            catch
//            {
//                // Uh, that was weird...
//            }
        }

        private class RotatableUIImagePickerController : UIImagePickerController
        {
            public override bool ShouldAutorotate()
            {
                return true;
            }

            public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
            {
                return UIInterfaceOrientationMask.All;
            }

            public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
            {
                return UIApplication.SharedApplication.StatusBarOrientation;
            }
        }

        private void SelectImage()
        {
            var imagePicker = new RotatableUIImagePickerController();
            imagePicker.NavigationControllerDelegate = new ImagePickerDelegate();
            imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
            imagePicker.MediaTypes = imagePicker.MediaTypes.Where(x => !(x.Contains("movie") || x.Contains("video"))).ToArray();

            imagePicker.FinishedPickingMedia += (sender, e) =>
            {
                // determine what was selected, video or image
                bool isImage = false;
                switch(e.Info[UIImagePickerController.MediaType].ToString()) {
                    case "public.image":
                        isImage = true;
                        break;
                }

                // if it was an image, get the other image info
                if(isImage) 
                {
                    // get the original image
                    UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                    if(originalImage != null) {
                        // do something with the image
                        try
                        { UploadImage(originalImage);
                        }
                        catch
                        {
                        }
                    }
                } 
                else 
                { // if it's a video
                    _alertDialogService.Alert("Not supported!", "Video upload is currently not supported.");
                }          

                // dismiss the picker
                imagePicker.DismissViewController(true, null);
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            };


            imagePicker.Canceled += (sender, e) =>
            {
                imagePicker.DismissViewController(true, null);
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            };

            NavigationController.PresentViewController(imagePicker, true, null);
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            if (_viewSegment.SelectedSegment == 0)
            {
                if (_previewView != null)
                {
                    _previewView.RemoveFromSuperview();
                    _previewView.Dispose();
                    _previewView = null;
                }

                Add(TextView);
                TextView.BecomeFirstResponder();
            }
            else
            {
                if (_previewView == null)
                    _previewView = new UIWebView(this.View.Bounds);

                TextView.RemoveFromSuperview();
                Add(_previewView);

                var markdownService = Locator.Current.GetService<IMarkdownService>();
                var markdownView = new MarkdownView() { Model = markdownService.Convert(TextView.Text) };
                _previewView.LoadHtmlString(markdownView.GenerateString(), null);
            }
        }
    }
}

