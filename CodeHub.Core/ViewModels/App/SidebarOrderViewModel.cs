using System.Collections.Generic;
using Xamarin.Utilities.Core.ViewModels;

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

