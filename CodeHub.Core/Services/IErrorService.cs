using System;

namespace CodeHub.Core.Services
{
    public interface IErrorService
    {
        void Log(Exception e, bool fatal = false);

        void Init();
    }
}

