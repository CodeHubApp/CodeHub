using System;
using MonoTouch.UIKit;
using Newtonsoft.Json;
using System.IO;
using MonoTouch.Foundation;
using System.Linq;

namespace CodeHub.iOS.Themes
{
    public class Theme
    {
        public UIColor PrimaryNavigationBarColor { get; set; }
        public UIColor PrimaryNavigationBarTextColor { get; set; }

        public static string[] AvailableThemes
        {
            get
            {
                var path = Path.Combine(NSBundle.MainBundle.BundlePath, "Themes");
                return Directory.GetFiles(path, "*.json").Select(x => x.Replace(".json", string.Empty)).ToArray();
            }
        }

        public static Theme Load(string name)
        {
            var path = Path.Combine(NSBundle.MainBundle.BundlePath, "Themes", name + ".json");
            var themeText = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Theme>(themeText, new UIColorJsonConverter());
        }

        private class UIColorJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(UIColor);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType != JsonToken.String)
                    return null;
                var str = reader.Value as string;
                str = str.Substring(str.IndexOf("0x", StringComparison.Ordinal) + 2);
                var r = Convert.ToByte(str.Substring(0, 2), 16);
                var g = Convert.ToByte(str.Substring(2, 2), 16);
                var b = Convert.ToByte(str.Substring(4, 2), 16);
                return UIColor.FromRGB(r, g, b);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}

