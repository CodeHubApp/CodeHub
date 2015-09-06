using Foundation;

namespace CodeHub.ViewControllers
{
	public class GistFileView : CodeHub.iOS.Views.Source.FileSourceView
    {
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ViewModel.Bind(x => x.ContentPath, x =>
			{
				var data = System.IO.File.ReadAllText(x, System.Text.Encoding.UTF8);
				LoadContent(data, System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "SourceBrowser"));
			});
		}
    }
}

