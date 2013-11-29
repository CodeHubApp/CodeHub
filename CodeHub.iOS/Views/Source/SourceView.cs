using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views.Source
{
	public class SourceView : FileSourceView
    {
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ViewModel.Bind(x => x.IsLoading, x =>
			{
				if (x) return;
				if (!string.IsNullOrEmpty(ViewModel.ContentPath))
				{
					var data = System.IO.File.ReadAllText(ViewModel.ContentPath, System.Text.Encoding.UTF8);
					LoadContent(data, System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "SourceBrowser"));
				}
				else if (!string.IsNullOrEmpty(ViewModel.FilePath))
				{
					LoadFile(ViewModel.FilePath);
				}
			});
		}
    }
}

