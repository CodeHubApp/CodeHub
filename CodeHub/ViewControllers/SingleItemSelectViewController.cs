using System;
using System.Collections.Generic;
using System.Linq;
using CodeHub.Controllers;
using CodeFramework.Controllers;

namespace CodeHub.ViewControllers
{
    public class ComponentSelectViewController : SingleItemSelectViewController
    {
        public string Username { get; set; }
        public string Repo { get; set; }
        public string SelectedItem { get; set; }
        public ComponentSelectViewController() 
            : base("Components") 
        { 
        }

        protected override void Refresh()
        {
            this.DoWork(Do);
        }

        private void Do()
        {
//            var comp = Application.Client.Users[Username].Repositories[Repo].Issues.GetComponents();
//            BeginInvokeOnMainThread(() => {
//                SetValues(from x in comp select x.Name, SelectedItem);
//            });
        }
    }

    public abstract class SingleItemSelectViewController : RadioChoiceViewController
    {
        private bool _didLoad;

        public SingleItemSelectViewController(string title)
            : base(title)
        {
        }

        protected abstract void Refresh();

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (!_didLoad)
            {
                Refresh();
                _didLoad = true;
            }
        }
    }
}

