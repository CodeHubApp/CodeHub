using System;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace CodeHub.Core.Utilities
{
    public class UnderscoreContractResolver : DefaultContractResolver 
    {
        protected override string ResolvePropertyName(string propertyName) 
        {
            var sb = new StringBuilder();
            foreach (var c in propertyName)
            {
                if (char.IsLower(c))
                    sb.Append(c);
                else if (sb.Length == 0)
                    sb.Append(char.ToLower(c));
                else
                    sb.Append('_').Append(char.ToLower(c));
            }
            return sb.ToString();
        }
    }
}

