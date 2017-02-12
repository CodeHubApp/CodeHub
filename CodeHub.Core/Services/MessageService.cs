using System;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public class MessageService : IMessageService
    {
        private readonly IList<Tuple<Type, WeakReference>> _subscriptions = new List<Tuple<Type, WeakReference>>();

        public IDisposable Listen<T>(Action<T> action)
        {
            var obj = Tuple.Create(typeof(T), new WeakReference(action));
            lock (_subscriptions)
                _subscriptions.Add(obj);
            return new Reference(action, () => _subscriptions.Remove(obj));
        }

        public void Send<T>(T message)
        {
            lock (_subscriptions)
            {
                var shouldRemove = new LinkedList<Tuple<Type, WeakReference>>();

                foreach (var sub in _subscriptions)
                {
                    if (!sub.Item2.IsAlive)
                        shouldRemove.AddLast(sub);

                    if (sub.Item1 == typeof(T))
                    {
                        var handle = sub.Item2.Target;
                        if (handle != null)
                        {
                            ((Action<T>)handle).Invoke(message);
                        }
                    }
                }

                foreach (var r in shouldRemove)
                    _subscriptions.Remove(r);
            }
        }

        private class Reference : IDisposable
        {
            private readonly Action _removal;
            private readonly object _handle;

            public Reference(object handle, Action removal)
            {
                _handle = handle;
                _removal = removal;
            }

            public void Dispose() => _removal.Invoke();
        }
    }
}
