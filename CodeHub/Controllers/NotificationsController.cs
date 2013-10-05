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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Notifications.GetAll(all: _all, participating: _participating), forceDataRefresh, response => {
                RenderView(new ListModel<NotificationModel>(response.Data, this.CreateMore(response, callback: UpdateAccountNotificationsCount)));
                UpdateAccountNotificationsCount();
            });
        }

        public void Read(NotificationModel model)
        {
            var response = Application.Client.Execute(Application.Client.Notifications[model.Id].MarkAsRead());
            if (response.Data) 
            {
                //We just read it
                model.Unread = false;
                RenderView();

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

