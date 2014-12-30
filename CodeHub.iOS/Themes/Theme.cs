using System;
using MonoTouch.UIKit;
using Newtonsoft.Json;
using System.IO;
using MonoTouch.Foundation;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.SlideoutNavigation;
using CodeHub.iOS.ViewControllers;
using System.Reflection;

namespace CodeHub.iOS.Themes
{

    public class Theme
    {
        public UIColor PrimaryNavigationBarColor { get; set; }
        public UIColor PrimaryNavigationBarTextColor { get; set; }

        public UIColor IconColor { get; set; }
        public UIColor MenuIconColor { get; set; }
        public UIColor MenuTextColor { get; set; }
        public UIColor MenuBackgroundColor { get; set; }
        public UIColor MenuSelectedBackgroundColor { get; set; }

        private static Theme _current;
        public static Theme Current 
        {
            get
            {
                if (_current == null)
                {
                    _current = LoadTheme("Default");
                    Setup(_current);
                }
                return _current;
            }
            set
            {
                _current = value;
                Setup(_current);
            }
        }

        public static void Setup(Theme theme)
        {
            UIGraphics.BeginImageContext(new System.Drawing.SizeF(1, 64f));
            theme.PrimaryNavigationBarColor.SetFill();
            UIGraphics.RectFill(new System.Drawing.RectangleF(0, 0, 1, 64));
            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;

            var navBarContainers = new [] { typeof(MenuNavigationController), typeof(ThemedNavigationController), typeof(MainNavigationController) };
            foreach (var navbarAppearance in navBarContainers.Select(x => UINavigationBar.AppearanceWhenContainedIn(x)))
            {
                navbarAppearance.TintColor = theme.PrimaryNavigationBarTextColor;
                navbarAppearance.BarTintColor = theme.PrimaryNavigationBarColor;
                navbarAppearance.BackgroundColor = theme.PrimaryNavigationBarColor;
                navbarAppearance.SetTitleTextAttributes(new UITextAttributes { TextColor = theme.PrimaryNavigationBarTextColor, Font = UIFont.SystemFontOfSize(18f) });
                navbarAppearance.SetBackgroundImage(img, UIBarPosition.Any, UIBarMetrics.Default);
                navbarAppearance.BackIndicatorImage = Images.BackButton;
                navbarAppearance.BackIndicatorTransitionMaskImage = Images.BackButton;
            }

            UISegmentedControl.Appearance.TintColor = UIColor.FromRGB(110, 110, 117);
            UISegmentedControl.AppearanceWhenContainedIn(typeof(UINavigationBar)).TintColor = UIColor.White;

            UISwitch.Appearance.OnTintColor = UIColor.FromRGB(0x41, 0x83, 0xc4);

            // Composer Input Accessory Buttons
            UIButton.AppearanceWhenContainedIn(typeof(UIScrollView)).TintColor = theme.PrimaryNavigationBarColor;

            UITableViewHeaderFooterView.Appearance.TintColor = UIColor.FromRGB(228, 228, 228);
            UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).TextColor = UIColor.FromRGB(136, 136, 136);
            UILabel.AppearanceWhenContainedIn(typeof(UITableViewHeaderFooterView)).Font = UIFont.SystemFontOfSize(13f);

            UIToolbar.Appearance.BarTintColor = UIColor.FromRGB(245, 245, 245);

            UIBarButtonItem.AppearanceWhenContainedIn(typeof(UISearchBar)).SetTitleTextAttributes(new UITextAttributes {TextColor = UIColor.White}, UIControlState.Normal);
        }

        public static string[] AvailableThemes
        {
            get
            {
                var path = Path.Combine(NSBundle.MainBundle.BundlePath, "Themes");
                return Directory.GetFiles(path, "*.json").Select(x => x.Replace(".json", string.Empty)).ToArray();
            }
        }


        public static void Load(string name)
        {
            var baseTheme = LoadTheme("Default");
            if (name != "Default")
            {
                var superTheme = LoadTheme(name);
                var properties = typeof(Theme).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var p in properties)
                {
                    var fromVal = p.GetValue(superTheme);
                    if (fromVal != null)
                        p.SetValue(baseTheme, fromVal);
                }
            }

            Current = baseTheme;
        }

        private static Theme LoadTheme(string name)
        {
            var path = Path.Combine(NSBundle.MainBundle.BundlePath, "Themes", name + ".json");
            var themeText = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Theme>(themeText, new UIColorJsonConverter(), new UriJsonConverter());
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

        private class UriJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Uri);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType != JsonToken.String)
                    return null;
                var val = reader.Value as string;
                if (new [] { "http://", "https://" }.Any(x => val.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
                    return new Uri(val);
                return new Uri(Path.Combine(NSBundle.MainBundle.BundlePath, val));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}

