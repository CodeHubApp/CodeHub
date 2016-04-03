using System;
using System.Threading.Tasks;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Services;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistFileViewModel : FileSourceViewModel
    {
        private string _id;
        private string _filename;

        public string FileName { get; private set; }

        private GistFileModel _gist;
        public GistFileModel Gist
        {
            get { return _gist; }
            private set { this.RaiseAndSetIfChanged(ref _gist, value); }
        }

        public void Init(NavObject navObject)
        {
            //Create the filename
            var fileName = System.IO.Path.GetFileName(navObject.Filename);
            if (fileName == null)
                fileName = navObject.Filename.Substring(navObject.Filename.LastIndexOf('/') + 1);

            //Create the temp file path
            Title = fileName;
            FileName = fileName;

            _id = navObject.GistId;
            _filename = navObject.Filename;

            //Grab the data
            Gist = GetService<IViewModelTxService>().Get() as GistFileModel;
        }

        protected override async Task Load()
        {
            
            if (Gist == null)
            {
                var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Gists[_id].Get());
                Gist = data.Data.Files[_filename];
            }

            if (Gist == null || Gist.Content == null)
                throw new Exception("Unable to retreive gist!");

            IsMarkdown = string.Equals(Gist?.Language, "Markdown");
            Gist = Gist;

            var filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), FileName);
            System.IO.File.WriteAllText(filepath, Gist.Content, System.Text.Encoding.UTF8);
            ContentPath = FilePath = filepath;
        }

        public class NavObject
        {
            public string GistId { get; set; }
            public string Filename { get; set; }
        }
    }
}

