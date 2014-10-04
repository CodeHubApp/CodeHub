using System;
using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.Core.Services;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Core.Models;
using System.Collections.Generic;
using Xamarin.Utilities.Core;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.App
{
    public abstract class MarkdownComposerViewModel : ComposerViewModel, ISaveableViewModel
    {
        private const string AuthorizationClientId = "aa5d7d0bc1dffa6";

        public IReactiveCommand<ImgurModel> PostToImgurCommand { get; private set; }

        public IReactiveCommand SaveCommand { get; private set; }

        protected MarkdownComposerViewModel(IStatusIndicatorService status, IAlertDialogService alert, IJsonHttpClientService jsonClient)
        {
            var saveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                t => Save());
            saveCommand.IsExecuting.Where(x => x).Subscribe(_ => status.Show("Saving..."));
            saveCommand.IsExecuting.Where(x => !x).Subscribe(_ => status.Hide());
            saveCommand.Subscribe(x => DismissCommand.ExecuteIfCan());
            SaveCommand = saveCommand;

            PostToImgurCommand = 
                ReactiveCommand.CreateAsyncTask(async data =>
                {
                    var uploadData = data as byte[];
                    if (uploadData == null)
                        throw new Exception("There is no data to upload!");

                    return await jsonClient.Post<ImgurModel>("https://api.imgur.com/3/image", 
                        new { image = Convert.ToBase64String(uploadData) }, 
                        new Dictionary<string, string> { {"Authorization", "Client-ID " + AuthorizationClientId} });
                });

            PostToImgurCommand.IsExecuting.Where(x => x).Subscribe(_ => status.Show("Uploading..."));
            PostToImgurCommand.IsExecuting.Where(x => !x).Subscribe(_ => status.Hide());
            PostToImgurCommand.ThrownExceptions.SubscribeSafe(e => alert.Alert("Error", "Unable to upload image: " + e.Message));
        }

        protected abstract Task Save();
    }
}