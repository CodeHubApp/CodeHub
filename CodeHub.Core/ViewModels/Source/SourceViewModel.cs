using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using GitHubSharp.Models;
using System;
using CodeFramework.Core.ViewModels;
using Cirrious.MvvmCross.Plugins.Messenger;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Source
{
	public class SourceViewModel : FileSourceViewModel
    {
		private MvxSubscriptionToken _editToken;

		private string _path;
		private string _name;
		private string _gitUrl;
		private bool _forceBinary;

		public string Username { get; private set; }

		public string Repository { get; private set; }

		public string Branch { get; private set; }

		public bool TrueBranch { get; private set; }

		protected override async Task Load(bool forceCacheInvalidation)
        {
			var filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(_name));
			string mime = string.Empty;

			using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
			{
				mime = await this.GetApplication().Client.DownloadRawResource2(_gitUrl, stream) ?? string.Empty;
			}

			FilePath = filepath;

			// We can force a binary representation if it was passed during init. In which case we don't care to figure out via the mime.
			if (_forceBinary)
				return;

			var isText = mime.Contains("charset");
			if (isText)
			{
				ContentPath = CreateContentFile();
			}
        }

		public ICommand GoToEditCommand
		{
			get { return new MvxCommand(() => ShowViewModel<EditSourceViewModel>(new EditSourceViewModel.NavObject { Path = _path, Branch = Branch, Username = Username, Repository = Repository }), () => TrueBranch); }
		}

		public void Init(NavObject navObject)
		{
			_path = navObject.Path;
			HtmlUrl = navObject.HtmlUrl;
			_name = navObject.Name;
			_gitUrl = navObject.GitUrl;
			_forceBinary = navObject.ForceBinary;
			Username = navObject.Username;
			Repository = navObject.Repository;
			Branch = navObject.Branch;
			TrueBranch = navObject.TrueBranch;

			//Create the filename
			var fileName = System.IO.Path.GetFileName(_path);
			if (fileName == null)
				fileName = _path.Substring(_path.LastIndexOf('/') + 1);

			//Create the temp file path
			Title = fileName;

			_editToken = Messenger.SubscribeOnMainThread<SourceEditMessage>(x =>
			{
				if (x.OldSha == null || x.Update == null)
					return;
				_gitUrl = x.Update.Content.GitUrl;
				LoadCommand.Execute(true);
			});
		}

		public class NavObject
		{
			public string Username { get; set; }
			public string Repository { get; set; }
			public string Branch { get; set; }
			public string Path { get; set; }
			public string HtmlUrl { get; set; }
			public string Name { get; set; }
			public string GitUrl { get; set; }
			public bool ForceBinary { get; set; }
			public bool TrueBranch { get; set; }
		}
    }
}