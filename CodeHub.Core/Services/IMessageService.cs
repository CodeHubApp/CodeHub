using System;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public interface IMessageService
    {
        void Send<T>(T message);

        IDisposable Listen<T>(Action<T> action);
    }
}