using System.Threading.Tasks;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IImgurService
    {
        Task<ImgurResponse> SendImage(byte[] data);
    }
}

