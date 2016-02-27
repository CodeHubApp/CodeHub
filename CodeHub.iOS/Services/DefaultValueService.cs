using CodeHub.Core.Services;
using Foundation;

namespace CodeHub.iOS.Services
{
    public class DefaultValueService : IDefaultValueService
    {
        public static NSUserDefaults Defaults = NSUserDefaults.StandardUserDefaults;

        public bool TryGet(string key, out string value)
        {
            if (Defaults[key] == null)
            {
                value = default(string);
                return false;
            }

            value = Defaults.StringForKey(key);
            return true;
        }

        public bool TryGet(string key, out int value)
        {
            if (Defaults[key] == null)
            {
                value = default(int);
                return false;
            }

            value = (int)Defaults.IntForKey(key);
            return true;
        }

        public bool TryGet(string key, out bool value)
        {
            if (Defaults[key] == null)
            {
                value = default(bool);
                return false;
            }

            value = Defaults.BoolForKey(key);
            return true;
        }

        public void Set(string key, string value)
        {
            if (value == null)
                Clear(key);
            else
            {
                Defaults.SetString(value, key);
                Defaults.Synchronize();
            }
        }

        public void Set(string key, int? value)
        {
            if (!value.HasValue)
                Clear(key);
            else
            {
                Defaults.SetInt(value.Value, key);
                Defaults.Synchronize();
            }
        }

        public void Set(string key, bool? value)
        {
            if (!value.HasValue)
                Clear(key);
            else
            {
                Defaults.SetBool(value.Value, key);
                Defaults.Synchronize();
            }
        }

        public void Clear(string key)
        {
            Defaults.RemoveObject(key);
            Defaults.Synchronize();
        }
    }
}
