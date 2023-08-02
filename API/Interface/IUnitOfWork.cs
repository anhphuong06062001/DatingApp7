namespace API.Interface
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository {get;}
        IMessageReponsitory MessageReponsitory {get;}
        ILikesRespository LikesRespository {get;}
        Task<bool> Complete();
        bool HasChanges();   
    }
}