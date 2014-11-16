using System.Threading.Tasks;
using Splat;

namespace CodeHub.Core.Services
{
    public interface IMediaPickerService
    {
        Task<IBitmap> PickPhoto();
    }
}

