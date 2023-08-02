using API.Interface;
using API.Serviecs;
using AutoMapper;

namespace API.Data
{
      public class UnitOfWork : IUnitOfWork
      {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            public UnitOfWork(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }
            public IUserRepository UserRepository => new UserRepository(_context, _mapper);

            public IMessageReponsitory MessageReponsitory => new MessageReponsitory(_context, _mapper);

            public ILikesRespository LikesRespository => new LikesRepository(_context);

            public async Task<bool> Complete()
            {
                return await _context.SaveChangesAsync() > 0;
            }

            public bool HasChanges()
            {
                return _context.ChangeTracker.HasChanges();
            }
      }
}