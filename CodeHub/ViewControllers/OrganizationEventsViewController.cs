using System;
using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
    public class OrganizationEventsViewController : BaseEventsViewController
    {
        public OrganizationEventsViewController(string userName, string orgName)
        {
            Controller = new OrganizationEventsController(this, userName, orgName);
        }
    }
}

