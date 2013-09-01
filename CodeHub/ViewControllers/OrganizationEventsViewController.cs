using System;
using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
    public class OrganizationEventsViewController : BaseEventsViewController
    {
        public OrganizationEventsViewController(string userName, string orgName)
        {
            Title = userName;
            Controller = new OrganizationEventsController(this, userName, orgName);
        }
    }
}

