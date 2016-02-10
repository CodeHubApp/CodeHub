using CodeHub.Core.Services;
using MonoTouch;

namespace CodeHub.iOS.Services
{
    public class DefaultValueService : IDefaultValueService
    {
        public bool TryGet(string key, out string value)
        {
            if (Utilities.Defaults[key] == null)
            {
                value = default(string);
                return false;
            }

            value = Utilities.Defaults.StringForKey(key);
            return true;
        }

        public bool TryGet(string key, out int value)
        {
            if (Utilities.Defaults[key] == null)
            {
                value = default(int);
                return false;
            }

            value = (int)Utilities.Defaults.IntForKey(key);
            return true;
        }

        public bool TryGet(string key, out bool value)
        {
            if (Utilities.Defaults[key] == null)
            {
                value = default(bool);
                return false;
            }

            value = Utilities.Defaults.BoolForKey(key);
            return true;
        }

        public void Set(string key, string value)
        {
            if (value == null)
                Clear(key);
            else
            {
                Utilities.Defaults.SetString(value, key);
                Utilities.Defaults.Synchronize();
            }
        }

        public void Set(string key, int? value)
        {
            if (!value.HasValue)
                Clear(key);
            else
            {
                Utilities.Defaults.SetInt(value.Value, key);
                Utilities.Defaults.Synchronize();
            }
        }

        public void Set(string key, bool? value)
        {
            if (!value.HasValue)
                Clear(key);
            else
            {
                Utilities.Defaults.SetBool(value.Value, key);
                Utilities.Defaults.Synchronize();
            }
        }

        public void Clear(string key)
        {
            Utilities.Defaults.RemoveObject(key);
            Utilities.Defaults.Synchronize();
        }
    }
}
