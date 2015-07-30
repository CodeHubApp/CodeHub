namespace CodeHub.Core.Services
{
    public interface IEnvironmentalService
    {
        string OSVersion { get; }

        string ApplicationVersion { get; }
    }
}

