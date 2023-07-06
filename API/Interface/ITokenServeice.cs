
using API.Entities;

namespace API.Interface
{
    public interface ITokenServeice
    {
        string CreateToken(AppUser user);
    }
}