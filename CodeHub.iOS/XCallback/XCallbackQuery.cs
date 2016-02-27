using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CodeHub.iOS.XCallback
{
    public class XCallbackQuery
    {
        public string SuccessUrl { get; private set; }
        public string ErrorUrl { get; private set; }
        public string CancelUrl { get; private set; }
        public string Url { get; private set; }
        public IDictionary<string, string> Parameters { get; private set; }

        public XCallbackQuery(string urlString)
        {
            var uri = new Uri(urlString);
            Url = uri.AbsolutePath;
            Parameters = new Dictionary<string, string>();
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            foreach (string key in query.Keys)
            {
                if (key == null)
                    continue;

                if (string.Equals("x-success", key, StringComparison.OrdinalIgnoreCase))
                    SuccessUrl = query[key];
                else if (string.Equals("x-error", key, StringComparison.OrdinalIgnoreCase))
                    ErrorUrl = query[key];
                else if (string.Equals("x-cancel", key, StringComparison.OrdinalIgnoreCase))
                    CancelUrl = query[key];
                else
                    Parameters.Add(key, query[key]);
            }
        }

        public string ExpandErrorUrl(int errorCode, string errorMessage)
        {
            return string.Format(ErrorUrl + "?errorCode={0}&errorMessage={1}", errorCode.ToString(), Uri.EscapeDataString(errorMessage));
        }

        public string ExpandSuccessUrl(IDictionary<string, string> parameters)
        {
            var sb = new StringBuilder();
            sb.Append(SuccessUrl);
            sb.Append("?");
            sb.Append(string.Join("&", parameters.Select(x => x.Key + "=" + Uri.EscapeDataString(x.Value))));
            return sb.ToString();
        }
    }
}

