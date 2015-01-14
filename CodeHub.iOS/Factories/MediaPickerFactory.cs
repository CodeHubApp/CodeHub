using System;
using System.Threading.Tasks;
using Splat;
using MonoTouch.UIKit;
using System.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.Factories
{
    public class MediaPickerFactory : IMediaPickerFactory
    {
        public async Task<IBitmap> PickPhoto()
        {
            var tcs = new TaskCompletionSource<IBitmap>();
            var imagePicker = new UIImagePickerController();
            imagePicker.NavigationControllerDelegate = new ImagePickerDelegate();
            imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
            imagePicker.MediaTypes = imagePicker.MediaTypes.Where(x => !(x.Contains("movie") || x.Contains("video"))).ToArray();

            imagePicker.FinishedPickingMedia += (sender, e) =>
            {
                try
                {
                    // determine what was selected, video or image
                    bool isImage = false;
                    switch(e.Info[UIImagePickerController.MediaType].ToString()) {
                        case "public.image":
                            isImage = true;
                            break;
                    }

                    // if it was an image, get the other image info
                    if(!isImage) 
                        throw new InvalidOperationException("Video upload is currently not supported.");

                    // get the original image
                    var originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                    if(originalImage == null) 
                        throw new InvalidOperationException("Unable to retrieve image from picker!");

                    // dismiss the picker
                    imagePicker.DismissViewController(true, null);
                    UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;

                    tcs.SetResult(originalImage.FromNative());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            imagePicker.Canceled += (sender, e) => tcs.SetCanceled();

            UIApplication.SharedApplication.KeyWindow.GetVisibleViewController().PresentViewController(imagePicker, true, null);

            try
            {
                return await tcs.Task;
            }
            finally
            {
                imagePicker.DismissViewController(true, null);
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            }
        }

        private class ImagePickerDelegate : UINavigationControllerDelegate
        {
            public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
            {
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            }
        }
    }
}

