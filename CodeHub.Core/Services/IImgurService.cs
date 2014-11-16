using System.Threading.Tasks;
using CodeHub.Core.Models;

namespace CodeHub.Core.Services
{
    public interface IImgurService
    {
        Task<ImgurModel> SendImage(byte[] data);
    }
}

