using CodeFramework.ViewControllers;
using MonoTouch.Dialog;
using CodeHub.Core.ViewModels.Issues;
using GitHubSharp.Models;
using System.Linq;
using MonoTouch.UIKit;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueLabelsView : ViewModelCollectionDrivenViewController
    {
		public IssueLabelsView()
		{
			EnableSearch = false;
			Title = "Labels".t();
			NoItemsText = "No Labels".t();
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			var vm = (IssueLabelsViewModel)ViewModel;
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.BackButton, UIBarButtonItemStyle.Plain, (s, e) => vm.SaveLabelChoices.Execute(null));

			BindCollection(vm.Labels, x => 
            {
				var e = new LabelElement(x);
                e.Tapped += () =>
                {
                    if (e.Accessory == UITableViewCellAccessory.Checkmark)
						vm.SelectedLabels.Items.Remove(x);
                    else
						vm.SelectedLabels.Items.Add(x);
                };

				e.Accessory = vm.SelectedLabels.Contains(x) ? 
				               UITableViewCellAccessory.Checkmark : 
				               UITableViewCellAccessory.None;
                return e;
            });

			vm.BindCollection(x => x.SelectedLabels, x =>
			{
				if (Root.Count == 0)
					return;

				var elements = Root[0].Elements;
				foreach (var el in elements.Cast<LabelElement>())
				{
					el.Accessory = vm.SelectedLabels.Contains(el.Label) ? 
					               UITableViewCellAccessory.Checkmark : 
					               UITableViewCellAccessory.None;
				}

				Root.Reload(Root[0], UITableViewRowAnimation.None);
			}, true);

			var _hud = new Hud(View);
			vm.Bind(x => x.IsSaving, x =>
			{
				if (x) _hud.Show("Saving...");
				else _hud.Hide();
			});
        }

		private class LabelElement : StyledStringElement
		{
			public LabelModel Label { get; private set; }
			public LabelElement(LabelModel m)
				: base(m.Name)
			{
				Label = m;
				Image = CreateImage(m.Color);
			}

			private static UIImage CreateImage(string color)
			{
				try
				{
					var red = color.Substring(0, 2);
					var green = color.Substring(2, 2);
					var blue = color.Substring(4, 2);

					var redB = System.Convert.ToByte(red, 16);
					var greenB = System.Convert.ToByte(green, 16);
					var BlueB = System.Convert.ToByte(blue, 16);

					var size = new System.Drawing.SizeF(24f, 24f);

					UIGraphics.BeginImageContext(size);
					UIColor.FromRGB(redB, greenB, BlueB).SetFill();
					GraphicsUtil.FillRoundedRect(UIGraphics.GetCurrentContext(), new System.Drawing.RectangleF(0, 0, size.Width, size.Height), 6f);
					var image = UIGraphics.GetImageFromCurrentImageContext();
					UIGraphics.EndImageContext();
					return image;
				}
				catch
				{
					return null;
				}
			}
		}
    }
}

