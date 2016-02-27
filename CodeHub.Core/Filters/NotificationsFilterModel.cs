using CodeHub.Core.ViewModels;

namespace CodeHub.Core.Filters
{
    public class NotificationsFilterModel : FilterModel<NotificationsFilterModel>
    {
        public bool All { get; set; }

        public bool Participating { get; set; }

        public NotificationsFilterModel()
        {
            All = false;
            Participating = false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(NotificationsFilterModel))
                return false;
            var other = (NotificationsFilterModel)obj;
            return All == other.All && Participating == other.Participating;
        }
        

        public override int GetHashCode()
        {
            unchecked
            {
                return All.GetHashCode() ^ Participating.GetHashCode();
            }
        }

        public override NotificationsFilterModel Clone()
        {
            return (NotificationsFilterModel)this.MemberwiseClone();
        }

        public static NotificationsFilterModel CreateUnreadFilter()
        {
            return new NotificationsFilterModel { All = false, Participating = false };
        }

        public static NotificationsFilterModel CreateParticipatingFilter()
        {
            return new NotificationsFilterModel { All = false, Participating = true };
        }

        public static NotificationsFilterModel CreateAllFilter()
        {
            return new NotificationsFilterModel { All = true, Participating = false };
        }
    }
}

