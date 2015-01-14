using System;
using System.Text;

// Analysis disable once CheckNamespace
namespace Newtonsoft.Json.Serialization
{
    public class UnderscoreContractResolver : DefaultContractResolver 
    {
        protected override string ResolvePropertyName(string propertyName) 
        {
            var sb = new StringBuilder();
            foreach (var c in propertyName.ToCharArray())
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

