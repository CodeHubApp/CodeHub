using Splat;
using System.Threading.Tasks;

namespace CodeHub.Core.Factories
{
    public interface IMediaPickerFactory
    {
        Task<IBitmap> PickPhoto();
    }
}

