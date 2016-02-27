using System;
using CodeHub.iOS.ViewControllers;
using UIKit;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using Foundation;
using System.Net;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;
using CodeHub.iOS.Services;
using CodeHub.iOS.WebViews;
using WebKit;

namespace CodeHub.iOS.ViewControllers
{
    public class MarkdownComposerViewController : Composer
    {
        private readonly UISegmentedControl _viewSegment;
        private WKWebView _previewView;

        public MarkdownComposerViewController()
        {
            _viewSegment = new UISegmentedControl(new [] { "Compose", "Preview" });
            _viewSegment.SelectedSegment = 0;
            NavigationItem.TitleView = _viewSegment;
            _viewSegment.ValueChanged += SegmentValueChanged;

            var cameraImage = Octicon.DeviceCamera.ToImage(25, false);
            var linkImage = Octicon.Link.ToImage(25, false);
            var pictureImage = Octicon.FileMedia.ToImage(25, false);

            var buttons = new []
            {
                CreateAccessoryButton("@", () => TextView.InsertText("@")),
                CreateAccessoryButton("#", () => TextView.InsertText("#")),
                CreateAccessoryButton("*", () => TextView.InsertText("*")),
                CreateAccessoryButton("`", () => TextView.InsertText("`")),
                CreateAccessoryButton(pictureImage, () => {
                    var range = TextView.SelectedRange;
                    TextView.InsertText("![]()");
                    TextView.SelectedRange = new NSRange(range.Location + 4, 0);
                }),
                CreateAccessoryButton(cameraImage, SelectImage),
                CreateAccessoryButton(linkImage, () => {
                    var range = TextView.SelectedRange;
                    TextView.InsertText("[]()");
                    TextView.SelectedRange = new NSRange(range.Location + 1, 0);
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
        }

        private class ImagePickerDelegate : UINavigationControllerDelegate
        {
            public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
            {
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            }
        }

        private class ImgurModel
        {
            public ImgurDataModel Data { get; set; }
            public bool Success { get; set; }

            public class ImgurDataModel
            {
                public string Link { get; set; }
            }
        }

        private async void UploadImage(UIImage img)
        {
            var hud = new CodeHub.iOS.Utilities.Hud(null);
            hud.Show("Uploading...");

            try
            {
                var returnData = await Task.Run<byte[]>(() => 
                {
                    using (var w = new WebClient())
                    {
                        var data = img.AsJPEG();
                        byte[] dataBytes = new byte[data.Length];
                        System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));

                        w.Headers.Set("Authorization", "Client-ID aa5d7d0bc1dffa6");

                        var values = new NameValueCollection
                        {
                            { "image", Convert.ToBase64String(dataBytes) }
                        };

                        return w.UploadValues("https://api.imgur.com/3/image", values);
                    }
                });


                var json = Mvx.Resolve<IJsonSerializationService>();
                var imgurModel = json.Deserialize<ImgurModel>(System.Text.Encoding.UTF8.GetString(returnData));
                TextView.InsertText("![](" + imgurModel.Data.Link + ")");
            }
            catch (Exception e)
            {
                AlertDialogService.ShowAlert("Error", "Unable to upload image: " + e.Message);
            }
            finally
            {
                hud.Hide();
            }
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
                        {
                            UploadImage(originalImage);
                        }
                        catch
                        {
                        }
                    }
                } 
                else 
                { // if it's a video
                    AlertDialogService.ShowAlert("Not supported!", "Video upload is currently not supported.");
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
                    _previewView = new WKWebView(this.View.Bounds, new WKWebViewConfiguration());

                TextView.RemoveFromSuperview();
                Add(_previewView);

                var markdownService = Mvx.Resolve<IMarkdownService>();
                var markdownText = markdownService.Convert(Text);
                var model = new DescriptionModel(markdownText, (int)UIFont.PreferredSubheadline.PointSize);
                var view = new MarkdownView { Model = model }.GenerateString();
                _previewView.LoadHtmlString(view, NSBundle.MainBundle.BundleUrl);
            }
        }
    }
}

