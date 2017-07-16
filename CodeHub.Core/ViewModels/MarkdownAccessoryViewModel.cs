using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.IO;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;
using Splat;
using Plugin.Media.Abstractions;

namespace CodeHub.Core.ViewModels
{
    public class MarkdownAccessoryViewModel : ReactiveObject
    {
        private const string IMGUR_UPLOAD_WARN = "IMGUR_UPLOAD_WARN";
        private const string IMGUR_UPLOAD_WARN_MESSAGE = 
            "Because GitHub's image upload API is not public images you upload here are hosted by Imgur. " + 
            "Please be aware of this when posting confidential information";

        public ReactiveCommand<Unit, string> PostToImgurCommand { get; private set; }

        public MarkdownAccessoryViewModel(
            IImgurService imgurService = null, 
            IMedia mediaPicker = null, 
            IAlertDialogService alertDialog = null)
        {
            imgurService = imgurService ?? Locator.Current.GetService<IImgurService>();
            mediaPicker = mediaPicker ?? Plugin.Media.CrossMedia.Current;
            alertDialog = alertDialog ?? Locator.Current.GetService<IAlertDialogService>();

            PostToImgurCommand = ReactiveCommand.CreateFromTask(async _ => {
                
                if (!Settings.HasSeenImgurUploadWarn)
                {
                    Settings.HasSeenImgurUploadWarn = true;
                    await alertDialog.Alert("Please Read!", IMGUR_UPLOAD_WARN_MESSAGE);
                }

                var photo = await mediaPicker.PickPhotoAsync(new PickMediaOptions
                {
                    CompressionQuality = 80
                });

                var memoryStream = new MemoryStream();
                await photo.GetStream().CopyToAsync(memoryStream);
                     
                using (alertDialog.Activate("Uploading..."))
                {
                    var model = await imgurService.SendImage(memoryStream.ToArray());
                    if (model == null || model.Data == null || model.Data.Link == null)
                        throw new InvalidOperationException("Unable to upload to Imgur. Please try again later.");
                    return model.Data.Link;
                }
            });

            PostToImgurCommand.ThrownExceptions
                .Where(x => !(x is TaskCanceledException))
                .Subscribe(x => alertDialog.Alert("Upload Error", x.Message));
        }
    }
}

