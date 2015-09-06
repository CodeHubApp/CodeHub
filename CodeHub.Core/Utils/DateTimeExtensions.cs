namespace System
{
    public static class DateTimeExtensions
    {
		public static string ToDaysAgo(this DateTimeOffset d)
		{
			var dt = DateTimeOffset.Now.Subtract(d);

			if (dt.TotalDays >= 365)
			{
                var years = Convert.ToInt32(dt.TotalDays) / 365; 
				return years + (years > 1 ? " years ago" : " year ago");
			}
			if (dt.TotalDays >= 30)
			{
                var months = Convert.ToInt32(dt.TotalDays) / 30; 
				return months + (months > 1 ? " months ago" : " month ago");
			}
            if (dt.TotalDays > 1)
            {
                var days = Convert.ToInt32(dt.TotalDays);
                return days + (days > 1 ? " days ago" : " day ago");
            }

            if (dt.TotalHours > 1)
            {
                var hours = Convert.ToInt32(dt.TotalHours);
                return hours + (hours > 1 ? " hours ago" : " hour ago");
            }

            if (dt.TotalMinutes > 1)
            {
                var minutes = Convert.ToInt32(dt.TotalMinutes);
                return minutes + (minutes > 1 ? " minutes ago" : " minute ago");
            }

			return "moments ago";
		}

        public static int TotalDaysAgo(this DateTime d)
        {
            return Convert.ToInt32(Math.Round(DateTime.Now.Subtract(d.ToLocalTime()).TotalDays));
        }

		public static int TotalDaysAgo(this DateTimeOffset d)
		{
			return Convert.ToInt32(Math.Round(DateTimeOffset.Now.Subtract(d).TotalDays));
		}
    }
}

