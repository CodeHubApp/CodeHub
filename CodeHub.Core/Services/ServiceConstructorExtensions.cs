namespace CodeHub.Core.Services
{
    public static class ServiceConstructorExtensions
    {
        public static T Construct<T>(this IServiceConstructor serviceConstructor)
        {
            return (T)serviceConstructor.Construct(typeof(T));
        }
    }
}

