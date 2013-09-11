using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeHub.Filters.Models;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.Controllers
{
    public class NotificationsController : ListController<NotificationModel, NotificationsFilterModel>
    {
        private bool _all = false;
        private bool _participating = true;


        public NotificationsController(IListView<NotificationModel> view)
            : base(view)
        {
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Notifications.GetAll(force, all: _all, participating: _participating);
            Model = new ListModel<NotificationModel> {Data = response.Data, More = this.CreateMore(response, callback: UpdateAccountNotificationsCount)};
            UpdateAccountNotificationsCount();
        }

        public void Read(NotificationModel model)
        {
            if (Application.Client.Notifications[model.Id].MarkAsRead())
            {
                //We just read it
                model.Unread = false;
                Render();

                //Update the notifications count on the account
                UpdateAccountNotificationsCount();
            }
        }

        private void UpdateAccountNotificationsCount()
        {
            Application.Account.Notifications = Model.Data.Count(x => x.Unread == true);
        }

        protected override List<IGrouping<string, NotificationModel>> GroupModel(List<NotificationModel> model, NotificationsFilterModel filter)
        {
            IEnumerable<NotificationModel> query = model;
            if (_all == false)
                query = model.Where(x => x.Unread == true);
            return query.GroupBy(x => x.Repository.FullName).ToList();
        }

        protected override void SaveFilterAsDefault(NotificationsFilterModel filter)
        {
            //Do nothing
        }
    }
}

