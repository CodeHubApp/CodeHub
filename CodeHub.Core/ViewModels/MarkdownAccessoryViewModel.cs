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
        public IReactiveCommand<string> PostToImgurCommand { get; private set; }

        protected MarkdownAccessoryViewModel(IImgurService imgurService, IMediaPickerFactory mediaPicker, IAlertDialogFactory alertDialogFactory)
        {
            PostToImgurCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
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

