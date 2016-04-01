using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface IFeaturesService
    {
        bool IsProEnabled { get; }

        void ActivateProDirect();

        Task ActivatePro();

        Task RestorePro();
    }
}

