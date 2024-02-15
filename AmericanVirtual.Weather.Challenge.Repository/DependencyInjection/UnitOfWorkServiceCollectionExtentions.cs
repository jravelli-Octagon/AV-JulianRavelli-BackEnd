using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AmericanVirtual.Weather.Challenge.Repository.Interfaces;

namespace AmericanVirtual.Weather.Challenge.Repository.DependencyInjection
{
    public static class UnitOfWorkServiceCollectionExtentions
    {
        public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<IRepositoryFactory, UnitOfWork<TContext>>();
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
            return services;
        }
    }
}