
using API.Entities;

namespace API.Interface
{
    public interface ITokenServeice
    {
        Task<string> CreateToken(AppUser user);
    }
}