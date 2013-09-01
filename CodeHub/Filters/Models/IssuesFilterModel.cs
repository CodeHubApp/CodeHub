using System;
using CodeFramework.Filters.Models;

namespace CodeHub.Filters.Models
{
    public class IssuesFilterModel : FilterModel<IssuesFilterModel>
    {
        public string AssignedTo { get; set; }
        public string ReportedBy { get; set; }
        public StatusModel Status { get; set; }
        public KindModel Kind { get; set; }
        public PriorityModel Priority { get; set; }
        public Order OrderBy { get; set; }

        public IssuesFilterModel()
        {
            Kind = new KindModel();
            Status = new StatusModel();
            Priority = new PriorityModel();
            OrderBy = Order.Local_Id;
        }

        /// <summary>
        /// Predefined 'All' filter
        /// </summary>
        /// <returns>The all filter.</returns>
        public static IssuesFilterModel CreateAllFilter()
        {
            return new IssuesFilterModel();
        }

        /// <summary>
        /// Predefined 'Open' filter
        /// </summary>
        /// <returns>The open filter.</returns>
        public static IssuesFilterModel CreateOpenFilter()
        {
            return new IssuesFilterModel { 
                Status = new IssuesFilterModel.StatusModel { 
                    New = true, Open = true, OnHold = false, Resolved = false, 
                    Wontfix = false, Duplicate = false, Invalid = false 
                }
            };
        }

        /// <summary>
        /// Predefined 'Mine' filter
        /// </summary>
        /// <returns>The mine filter.</returns>
        /// <param name="username">Username.</param>
        public static IssuesFilterModel CreateMineFilter(string username)
        {
            return new IssuesFilterModel { AssignedTo = username };
        }


        public bool IsFiltering()
        {
            return !(string.IsNullOrEmpty(AssignedTo) && string.IsNullOrEmpty(ReportedBy) && Status.IsDefault() && Kind.IsDefault() && Priority.IsDefault());
        }

        public override IssuesFilterModel Clone()
        {
            var t = (IssuesFilterModel)this.MemberwiseClone();
            t.Status = this.Status.Clone();
            t.Priority = this.Priority.Clone();
            t.Kind = this.Kind.Clone();
            return t;
        }

        public enum Order : int
        { 
            [EnumDescription("Number")]
            Local_Id, 
            Title,
            [EnumDescription("Last Updated")]
            Utc_Last_Updated, 
            [EnumDescription("Created Date")]
            Created_On, 
            Version, 
            Milestone, 
            Component, 
            Status, 
            Priority 
        };

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(IssuesFilterModel))
                return false;
            IssuesFilterModel other = (IssuesFilterModel)obj;
            return object.Equals(AssignedTo, other.AssignedTo) && object.Equals(ReportedBy, other.ReportedBy) && object.Equals(Status, other.Status) && object.Equals(Kind, other.Kind) && object.Equals(Priority, other.Priority) && object.Equals(OrderBy, other.OrderBy);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (AssignedTo != null ? AssignedTo.GetHashCode() : 0) ^ (ReportedBy != null ? ReportedBy.GetHashCode() : 0) ^ (Status != null ? Status.GetHashCode() : 0) ^ (Kind != null ? Kind.GetHashCode() : 0) ^ (Priority != null ? Priority.GetHashCode() : 0) ^ OrderBy.GetHashCode();
            }
        }

        public class StatusModel
        {
            public bool New = true, Open = true, Resolved = true, OnHold = true, Invalid = true, Duplicate = true, Wontfix = true;

            public bool IsDefault()
            {
                return this.Equals(new StatusModel());
            }

            public StatusModel Clone()
            {
                return (StatusModel)this.MemberwiseClone();
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != typeof(StatusModel))
                    return false;
                StatusModel other = (StatusModel)obj;
                return New == other.New && Open == other.Open && Resolved == other.Resolved && OnHold == other.OnHold && Invalid == other.Invalid && Duplicate == other.Duplicate && Wontfix == other.Wontfix;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return New.GetHashCode() ^ Open.GetHashCode() ^ Resolved.GetHashCode() ^ OnHold.GetHashCode() ^ Invalid.GetHashCode() ^ Duplicate.GetHashCode() ^ Wontfix.GetHashCode();
                }
            }
        }

        public class KindModel
        {
            public bool Bug = true, Enhancement = true, Proposal = true, Task = true;

            public bool IsDefault()
            {
                return this.Equals(new KindModel());
            }

            public KindModel Clone()
            {
                return (KindModel)this.MemberwiseClone();
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != typeof(KindModel))
                    return false;
                KindModel other = (KindModel)obj;
                return Bug == other.Bug && Enhancement == other.Enhancement && Proposal == other.Proposal && Task == other.Task;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Bug.GetHashCode() ^ Enhancement.GetHashCode() ^ Proposal.GetHashCode() ^ Task.GetHashCode();
                }
            }
        }

        public class PriorityModel
        {
            public bool Trivial = true, Minor = true, Major = true, Critical = true, Blocker = true;

            public bool IsDefault()
            {
                return this.Equals(new PriorityModel());
            }

            public PriorityModel Clone()
            {
                return (PriorityModel)this.MemberwiseClone();
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != typeof(PriorityModel))
                    return false;
                PriorityModel other = (PriorityModel)obj;
                return Trivial == other.Trivial && Minor == other.Minor && Major == other.Major && Critical == other.Critical && Blocker == other.Blocker;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Trivial.GetHashCode() ^ Minor.GetHashCode() ^ Major.GetHashCode() ^ Critical.GetHashCode() ^ Blocker.GetHashCode();
                }
            }
            
        }
    }
}

