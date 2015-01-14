using System;
using ReactiveUI;
using CodeHub.Core.Services;
using System.IO;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels
{
    public abstract class MarkdownComposerViewModel : BaseViewModel, IComposerViewModel
    {
        private string _text;
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public IReactiveCommand<string> PostToImgurCommand { get; private set; }

        protected MarkdownComposerViewModel(IImgurService imgurService, IMediaPickerFactory mediaPicker, IStatusIndicatorService statusIndicatorService)
        {
            PostToImgurCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var photo = await mediaPicker.PickPhoto();
                var memoryStream = new MemoryStream();
                await photo.Save(Splat.CompressedBitmapFormat.Jpeg, 0.8f, memoryStream);
                using (statusIndicatorService.Activate("Uploading..."))
                {
                    var model = await imgurService.SendImage(memoryStream.ToArray());
                    if (model == null || model.Data == null || model.Data.Link == null)
                        throw new InvalidOperationException("Unable to upload to Imgur. Please try again later.");
                    return model.Data.Link;
                }
            });
        }
    }
}

