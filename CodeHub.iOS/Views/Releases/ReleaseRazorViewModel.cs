using GitHubSharp.Models;
using Humanizer;

namespace CodeHub.iOS.Views.Releases
{
    public class ReleaseRazorViewModel
    {
        public string Body { get; set; }

        public string Name { get; set; }

        public ReleaseModel Release { get; set; }

        public string ReleaseTime
        {
            get
            {
                return Release.PublishedAt.HasValue ? 
                    Release.PublishedAt.Value.UtcDateTime.Humanize() : 
                    Release.CreatedAt.UtcDateTime.Humanize();
            }
        }
    }
}

