
using API.Data;
using API.Interface;
using API.Serviecs;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
         IConfiguration config)
        {
            services.AddDbContext<DataContext>(options => {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            services.AddCors();
            services.AddScoped<ITokenServeice, TokenService>();
            
            return services;
        }
        
    }
}