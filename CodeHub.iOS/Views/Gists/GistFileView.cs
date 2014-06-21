using CodeHub.Core.ViewModels.Gists;
using MonoTouch.Foundation;
using ReactiveUI;

namespace CodeHub.iOS.Views.Gists
{
	public class GistFileView : Source.FileSourceView<GistFileViewModel>
    {
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

//            ViewModel.WhenAnyValue(x => x.ContentPath)
//
//			ViewModel.Bind(x => x.ContentPath, x =>
//			{
//				var data = System.IO.File.ReadAllText(x, System.Text.Encoding.UTF8);
//				LoadContent(data, System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "SourceBrowser"));
//			});
		}
    }
}

