using System;
using CodeFramework.iOS.ViewControllers;
using MonoTouch.UIKit;
using Cirrious.CrossCore;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using MonoTouch.Foundation;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using CodeFramework.Core.Services;
using System.Linq;
using System.Drawing;

namespace CodeHub.iOS.ViewControllers
{
    public class MarkdownComposerViewController : Composer
    {
        private readonly UISegmentedControl _viewSegment;
        private UIWebView _previewView;

        public MarkdownComposerViewController()
        {
            _viewSegment = new UISegmentedControl(new [] { "Compose", "Preview" });
            _viewSegment.SelectedSegment = 0;
            NavigationItem.TitleView = _viewSegment;
            _viewSegment.ValueChanged += SegmentValueChanged;

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
                CreateAccessoryButton(photoImage, () => SelectImage()),
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
            var hud = new CodeFramework.iOS.Utils.Hud(null);
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
                MonoTouch.Utilities.ShowAlert("Error", "Unable to upload image: " + e.Message);
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
                    MonoTouch.Utilities.ShowAlert("Not supported!", "Video upload is currently not supported.");
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

                var markdownService = Mvx.Resolve<IMarkdownService>();
                var path = MarkdownHtmlGenerator.CreateFile(markdownService.Convert(Text));
                var uri = Uri.EscapeUriString("file://" + path) + "#" + Environment.TickCount;
                _previewView.LoadRequest(new MonoTouch.Foundation.NSUrlRequest(new MonoTouch.Foundation.NSUrl(uri)));
            }
        }
    }
}

