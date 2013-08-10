using CodeHub.Controllers;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.UIKit;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using MonoTouch.Dialog;
using System;

namespace CodeHub.GitHub.Controllers.Notifications
{
    public class NotificationsController : BaseListModelController
    {
        private const string SavedSelection = "NOTIFICATION_SELECTION";
        private static string[] _sections = new [] { "Unread", "Participating", "All" };

        public NotificationsController()
            : base(typeof(List<NotificationModel>))
        {
//            MultipleSelections = _sections;
//            MultipleSelectionsKey = SavedSelection;
            Title = "Notifications";
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
//            var selected = 0;
//            InvokeOnMainThread(() => { selected = _segment.SelectedSegment; });
//            GitHubSharp.GitHubResponse<List<NotificationModel>> data = null;
//            nextPage = -1;
//
//            if (selected == 0)
//                data = Application.Client.API.GetNotifications();
//            else if (selected == 1)
//                data = Application.Client.API.GetNotifications(false, true);
//            else if (selected == 2)
//                data = Application.Client.API.GetNotifications(true, false);
//            else
                return new List<NotificationModel>();
            //return data.Data;
        }

        protected override MonoTouch.Dialog.Element CreateElement(object obj)
        {
            var notification = (NotificationModel)obj;
            var sse = new NameTimeStringElement() { 
                Time = notification.UpdatedAt.ToDaysAgo(), 
                String = notification.Subject.Title, 
                Lines = 4, 
            };

            sse.Name = notification.Repository.Name;
            return sse;
        }
    }
}

