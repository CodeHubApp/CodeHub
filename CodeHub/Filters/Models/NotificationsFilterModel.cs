using System;
using CodeFramework.Filters.Models;

namespace CodeHub.Filters.Models
{
	public class NotificationsFilterModel : FilterModel<NotificationsFilterModel>
    {
		public NotificationsFilterModel()
        {
        }

		public override NotificationsFilterModel Clone()
        {
			return (NotificationsFilterModel)this.MemberwiseClone();
        }
    }
}

