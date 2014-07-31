using System;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using CodeFramework.Core.ViewModels.Source;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistFileViewModel : FileSourceViewModel<string>
    {
        private string _filename;
        private string _id;
        private GistFileModel _gistFile;

	    public string Id
	    {
	        get { return _id; }
	        set { this.RaiseAndSetIfChanged(ref _id, value); }
	    }

	    public string Filename
	    {
	        get { return _filename; }
	        set { this.RaiseAndSetIfChanged(ref _filename, value); }
	    }

	    public GistFileModel GistFile
	    {
	        get { return _gistFile; }
	        set { this.RaiseAndSetIfChanged(ref _gistFile, value); }
	    }

        private IReactiveCommand _loadCommand;
        public override IReactiveCommand LoadCommand
        {
            get { return _loadCommand; }
        }

	    public GistFileViewModel(IApplicationService applicationService)
	    {
	        this.WhenAnyValue(x => x.Filename).Subscribe(x =>
	        {
	            Title = x == null ? "Gist" : x.Substring(x.LastIndexOf('/') + 1);
	        });

            _loadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                if (GistFile == null)
			    {
                    var data = await applicationService.Client.ExecuteAsync(applicationService.Client.Gists[_id].Get());
                    GistFile = data.Data.Files[_filename];
			    }

			    //Check to make sure...
                if (GistFile == null || GistFile.Content == null)
			    {
				    throw new Exception("Unable to retreive gist!");
			    }

                var content = GistFile.Content;
                var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(GistFile.Filename));
			    System.IO.File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
                SourceItem = new FileSourceItemViewModel { FilePath = filePath };
            });
	    }
    }
}

