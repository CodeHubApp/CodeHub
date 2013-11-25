using GitHubSharp.Models;
using CodeFramework.Views;
using CodeFramework.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core;

namespace CodeHub.ViewControllers
{
	public class GistFileView : WebView
    {
		public new GistFileViewModel ViewModel
		{
			get { return (GistFileViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public override void ViewDidLoad()
		{
			Title = "Gist";

			base.ViewDidLoad();
			ViewModel.Bind(x => x.ContentPath, x => LoadFile(x));
		}
    }
}

