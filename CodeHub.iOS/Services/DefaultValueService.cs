using System;
using Foundation;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Services
{
    public class DefaultValueService : IDefaultValueService
    {
        private static Lazy<DefaultValueService> _instance = new Lazy<DefaultValueService>(() => new DefaultValueService());
        public static NSUserDefaults Defaults = NSUserDefaults.StandardUserDefaults;

        public static DefaultValueService Instance
        {
            get { return _instance.Value; }
        }

        public DefaultValueService()
        {
        }

        public T Get<T>(string key)
        {
            if (typeof (T) == typeof(int))
                return (T)(object)Defaults.IntForKey(key);
            if (typeof (T) == typeof(bool))
                return (T)(object)Defaults.BoolForKey(key);
            if (typeof (T) == typeof (string))
                return (T) (object) Defaults.StringForKey(key);
            throw new Exception("Key does not exist in Default database.");
        }

        public void Set(string key, object value)
        {
            if (value == null)
                Defaults.RemoveObject(key);
            else if (value is int)
                Defaults.SetInt((int)value, key);
            else if (value is bool)
                Defaults.SetBool((bool)value, key);
            else if (value is string)
                Defaults.SetString((string)value, key);
            Defaults.Synchronize();
        }
    }
}
