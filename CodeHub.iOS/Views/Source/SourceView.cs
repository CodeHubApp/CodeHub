using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Source;

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

		protected override UIActionSheet CreateActionSheet(string title)
		{
			var vm = (SourceViewModel)ViewModel;
			var sheet = base.CreateActionSheet(title);
			var editButton = vm.GoToEditCommand.CanExecute(null) ? sheet.AddButton("Edit") : -1;
			sheet.Clicked += (sender, e) =>
			{
				if (e.ButtonIndex == editButton)
				{
					vm.GoToEditCommand.Execute(null);
				}
			};
			return sheet;
		}
    }
}

