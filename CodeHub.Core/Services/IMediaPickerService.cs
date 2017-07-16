using Splat;
using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface IMediaPickerService
    {
        Task<IBitmap> PickPhoto();
    }
}

