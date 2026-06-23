using CloudGame.Domain.Entities;
using CloudGame.Domain.Interfaces;
using CloudGame.Domain.Interfaces.Security;
using CloudGame.Infrastructure.Dapper;
using CloudGame.Infrastructure.Dapper.Contracts;
using CloudGame.Infrastructure.Dapper.Repositories;
using CloudGame.Infrastructure.EntityFramework;
using CloudGame.Infrastructure.EntityFramework.Repositories;
using CloudGame.Infrastructure.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace CloudGame.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer
                (
                    configuration.GetConnectionString("Default")
                )
                .UseSeeding((context, _) =>
                {
                    if (context.Set<User>().Any(s => s.IsAdmin))
                        return;

                    context.Set<User>().Add(
                        new User(
                            configuration["AppDefaultUserAdmin:Name"],
                            configuration["AppDefaultUserAdmin:Email"],
                            configuration["AppDefaultUserAdmin:Password"],
                            DateTime.Today.AddYears(-18),
                            true
                        )
                    );

                    context.SaveChanges();
                })
                .UseAsyncSeeding(async (context, _, ct) =>
                {
                    if (await context.Set<User>().AnyAsync(s => s.IsAdmin, ct))
                        return;                    

                    await context.Set<User>().AddAsync(new User(configuration["AppDefaultUserAdmin:Name"], configuration["AppDefaultUserAdmin:Email"], configuration["AppDefaultUserAdmin:Password"], DateTime.Today.AddYears(-18), true), ct);
                    await context.SaveChangesAsync(ct);
                })
            );

            services.AddScoped<IDbConnection>(sp => new SqlConnection(configuration.GetConnectionString("Default")));
            services.AddScoped<IDapperContext>(sp => new DapperContext(configuration));
            services.AddScoped<IUserWriteOnlyRepository, UserWriteOnlyRepository>();
            services.AddScoped<IUserReadOnlyRepository, UserReadOnlyRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<AppDbContext>()));
            services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();

            return services;
        }
    }
}
