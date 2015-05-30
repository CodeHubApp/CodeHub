using System;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;
using System.IO;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels
{
    public class MarkdownAccessoryViewModel : ReactiveObject
    {
        private const string IMGUR_UPLOAD_WARN = "IMGUR_UPLOAD_WARN";
        private const string IMGUR_UPLOAD_WARN_MESSAGE = 
            "Because GitHub's image upload API is not public images you upload here are hosted by Imgur. " + 
            "Please be aware of this when posting confidential information";

        public IReactiveCommand<string> PostToImgurCommand { get; private set; }

        public MarkdownAccessoryViewModel(
            IImgurService imgurService, 
            IMediaPickerFactory mediaPicker, 
            IAlertDialogFactory alertDialogFactory,
            IDefaultValueService defaultValueService)
        {
            PostToImgurCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                if (!defaultValueService.GetOrDefault(IMGUR_UPLOAD_WARN, false))
                {
                    defaultValueService.Set(IMGUR_UPLOAD_WARN, true);
                    await alertDialogFactory.Alert("Please Read!", IMGUR_UPLOAD_WARN_MESSAGE);
                }

                var photo = await mediaPicker.PickPhoto();
                var memoryStream = new MemoryStream();
                await photo.Save(Splat.CompressedBitmapFormat.Jpeg, 0.8f, memoryStream);
                using (alertDialogFactory.Activate("Uploading..."))
                {
                    var model = await imgurService.SendImage(memoryStream.ToArray());
                    if (model == null || model.Data == null || model.Data.Link == null)
                        throw new InvalidOperationException("Unable to upload to Imgur. Please try again later.");
                    return model.Data.Link;
                }
            });

            PostToImgurCommand.ThrownExceptions
                .Where(x => !(x is TaskCanceledException))
                .Subscribe(x => alertDialogFactory.Alert("Upload Error", x.Message));
        }
    }
}

