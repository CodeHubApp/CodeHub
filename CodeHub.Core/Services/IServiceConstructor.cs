using System;

namespace CodeHub.Core.Services
{
    public interface IServiceConstructor
    {
        object Construct(Type type);
    }

    public static class ServiceConstructorExtensions
    {
        public static T Construct<T>(this IServiceConstructor serviceConstructor)
        {
            return (T)serviceConstructor.Construct(typeof(T));
        }
    }
}

