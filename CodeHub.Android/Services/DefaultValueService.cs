using System;
using System.Collections.Generic;
using CodeHub.Core.Services;
using Android.Preferences;

namespace CodeHub.Android.Services
{
    class DefaultValueService : IDefaultValueService
    {
        public T Get<T>(string key)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            if (!prefs.Contains(key))
                throw new KeyNotFoundException("Shared preference does not exist for key " + key);
            if (typeof(T) == typeof(int))
                return (T)(object)prefs.GetInt(key, 0);
            if (typeof(T) == typeof(bool))
                return (T)(object)prefs.GetBoolean(key, false);
            if (typeof(T) == typeof(string))
                return (T)(object)prefs.GetString(key, null);
            throw new NotSupportedException("Unable to retrieve shared preference for type " + typeof(T).Name);
        }

        public void Set(string key, object value)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var edit = prefs.Edit();
            if (value == null)
                edit.Remove(key);
            else if (value is bool)
                edit.PutBoolean(key, (bool)value);
            else if (value is string)
                edit.PutString(key, (string)value);
            else if (value is int)
                edit.PutInt(key, (int)value);
            else
                throw new NotImplementedException("Cannot set shared preferences for type " + value.GetType().Name);
            edit.Apply();
        }
    }
}