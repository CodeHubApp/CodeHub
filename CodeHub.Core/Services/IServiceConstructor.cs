using System;

namespace CodeHub.Core.Services
{
    public interface IServiceConstructor
    {
        object Construct(Type type);
    }
}

