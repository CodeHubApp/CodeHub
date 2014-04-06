using System;
using CodeFramework.Core.ViewModels;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.App
{
    public class SidebarOrderViewModel : BaseViewModel
    {
        public List<string> Items
        {
            get;
            private set;
        }

        public SidebarOrderViewModel()
        {
            Items = new List<string>();
            Items.Add("Favorite Repositories");
            Items.Add("News");
            Items.Add("Issues");
            Items.Add("Notifications");
        }

    }
}

