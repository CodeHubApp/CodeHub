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

namespace CodeHub.GitHub.Controllers.Gists
{
    public class GistInfoController : BaseModelDrivenController
    {
        private readonly HeaderView _header;

        public string Id { get; private set; }

        public new GistModel Model 
        { 
            get { return (GistModel)base.Model; }
            set { base.Model = value; }
        }
        
        public GistInfoController(string id)
            : base(typeof(GistModel))
        {
            Id = id;
            Style = MonoTouch.UIKit.UITableViewStyle.Grouped;
            Title = "Gist";
            Root.UnevenRows = true;
            
            _header = new HeaderView(0f) { Title = "Gist: " + id };
            Root.Add(new Section(_header));
        }
        
        protected override void OnRender()
        {
            var sec = new Section();
            _header.Subtitle = "Updated " + Model.UpdatedAt.ToDaysAgo();

            var str = string.IsNullOrEmpty(Model.Description) ? "Gist " + Model.Id : Model.Description;
            var d = new NameTimeStringElement() { 
                Time = Model.UpdatedAt.ToDaysAgo(), 
                String = str, 
                Image = CodeFramework.Images.Misc.Anonymous,
                BackgroundColor = UIColor.White,
                UseBackgroundColor = true,
            };

            //Sometimes there's no user!
            d.Name = (Model.User == null) ? "Anonymous" : Model.User.Login;
            d.ImageUri = (Model.User == null) ? null : new Uri(Model.User.AvatarUrl);

            sec.Add(d);
            
            var sec2 = new Section();

            foreach (var file in Model.Files.Keys)
            {
                var sse = new SubcaptionElement(file, Model.Files[file].Size + " bytes") { 
                    Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator, 
                    LineBreakMode = MonoTouch.UIKit.UILineBreakMode.TailTruncation,
                    Lines = 1 
                };

                var fileSaved = file;
                sse.Tapped += () => NavigationController.PushViewController(new GistFileController(Model.Files[fileSaved]), true);
                sec2.Add(sse);
            }

            _header.SetNeedsDisplay();
            Root.Add(new [] { sec, sec2 });
            ReloadData();
        }

        protected override object OnUpdateModel(bool forced)
        {
            return Application.Client.API.GetGist(Id).Data;
        }
    }
}

