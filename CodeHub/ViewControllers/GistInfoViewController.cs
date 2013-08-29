using System;
using CodeHub.Controllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using CodeHub.Data;
using MonoTouch.UIKit;
using MonoTouch;
using CodeFramework.Controllers;
using CodeFramework.Views;
using CodeFramework.Elements;

namespace CodeHub.ViewControllers
{
    public class GistInfoViewController : BaseControllerDrivenViewController, IView<GistModel>
    {
        private readonly HeaderView _header;
        
        public GistInfoViewController(string id)
        {
            Title = "Gist";
            Controller = new GistInfoController(this, id);
            _header = new HeaderView(0f) { Title = "Gist: " + id };
        }

        public GistInfoViewController(GistModel model)
            : this (model.Id)
        {
            ((GistInfoController)Controller).Model = model;
        }

        public void Render(GistModel model)
        {
            var root = new RootElement(Title) { UnevenRows = true };
            var sec = new Section();
            _header.Subtitle = "Updated " + model.UpdatedAt.ToDaysAgo();


            var str = string.IsNullOrEmpty(model.Description) ? "Gist " + model.Id : model.Description;
            var d = new NameTimeStringElement() { 
                Time = model.UpdatedAt.ToDaysAgo(), 
                String = str, 
                Image = CodeFramework.Images.Misc.Anonymous,
                BackgroundColor = UIColor.White,
                UseBackgroundColor = true,
            };

            //Sometimes there's no user!
            d.Name = (model.User == null) ? "Anonymous" : model.User.Login;
            d.ImageUri = (model.User == null) ? null : new Uri(model.User.AvatarUrl);

            sec.Add(d);

            var sec2 = new Section();

            foreach (var file in model.Files.Keys)
            {
                var sse = new SubcaptionElement(file, model.Files[file].Size + " bytes") { 
                    Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator, 
                    LineBreakMode = MonoTouch.UIKit.UILineBreakMode.TailTruncation,
                    Lines = 1 
                };

                var fileSaved = file;
                var gistFileModel = model.Files[fileSaved];

                if (string.Equals(gistFileModel.Language, "markdown", StringComparison.OrdinalIgnoreCase))
                    sse.Tapped += () => NavigationController.PushViewController(new GistViewableFileController(gistFileModel), true);
                else
                    sse.Tapped += () => NavigationController.PushViewController(new GistFileViewController(gistFileModel), true);

                sec2.Add(sse);
            }

            _header.SetNeedsDisplay();
            root.Add(new [] { sec, sec2 });
            Root = root;
        }
    }
}

