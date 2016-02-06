using System;
using CodeHub.Core.Services;
using MonoTouch;

namespace CodeHub.iOS.Services
{
    public class DefaultValueService : IDefaultValueService
    {
        public T Get<T>(string key)
        {   
            if (Utilities.Defaults[key] == null)
                throw new Exception("Value for key '" + key + "' does not exist!");

            if (typeof(T) == typeof(int))
                return (T)(object)(int)Utilities.Defaults.IntForKey(key);
            if (typeof(T) == typeof(bool))
                return (T)(object)Utilities.Defaults.BoolForKey(key);
            if (typeof (T) == typeof (string))
                return (T) (object) Utilities.Defaults.StringForKey(key);
            throw new Exception("Key does not exist in Default database.");
        }

        public bool TryGet<T>(string key, out T value)
        {
            try
            {
                value = Get<T>(key);
                return true;
            }
            catch (Exception e)
            {
                value = default(T);
                return false;
            }

//            var val = Utilities.Defaults.ValueForKey(new MonoTouch.Foundation.NSString(key));
//            if (val == null)
//            {
//                value = default(T);
//                return false;
//            }
//            value = Get<T>(key);
//            return true;
        }

        public void Set(string key, object value)
        {
            if (value == null)
                Utilities.Defaults.RemoveObject(key);
            else if (value is int)
                Utilities.Defaults.SetInt((int)value, key);
            else if (value is bool)
                Utilities.Defaults.SetBool((bool)value, key);
            else if (value is string)
                Utilities.Defaults.SetString((string)value, key);
            Utilities.Defaults.Synchronize();
        }
    }
}
