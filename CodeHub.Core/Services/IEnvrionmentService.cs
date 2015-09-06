using System;

namespace CodeFramework.Core.Services
{
    public interface IEnvironmentService
    {
        string OSVersion { get; }

        string ApplicationVersion { get; }

        string DeviceName { get; }
    }
}

