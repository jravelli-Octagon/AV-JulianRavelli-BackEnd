using AmericanVirtual.Weather.Challenge.Repository.DependencyInjection;
using AmericanVirtual.Weather.Challenge.Common.Credentials;
using AmericanVirtual.Weather.Challenge.CoreAPI.Interfaces;
using AmericanVirtual.Weather.Challenge.CoreAPI.Mapper;
using AmericanVirtual.Weather.Challenge.CoreAPI.Services;
using AmericanVirtual.Weather.Challenge.Database.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AmericanVirtual.Weather.Challenge.WebAPI.Authorization;

namespace American_Virtual_Weather_Challenge.Dependencies
{
    public static class Dependencies
    {
        public static void ConfigureDependencies(IServiceCollection services, IConfiguration configuration)
        {
            string dbConnString = CredentialManager.GetConnectionString(configuration.GetSection("Database:UserName").Value, configuration.GetSection("Database:Password").Value, configuration.GetConnectionString("DefaultConnection"));
            services.AddDbContext<AmericaVirtualContext>(options =>
                    options.UseSqlServer(dbConnString, sqlServerOptions => sqlServerOptions.CommandTimeout(120)), ServiceLifetime.Scoped).AddUnitOfWork<AmericaVirtualContext>();

            services.AddHttpContextAccessor();
            //In-Memory
            services.AddDistributedMemoryCache();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(1);
            });

            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IWeatherService, WeatherService>();

            services.AddSingleton<IAutoMapper, AutoMapperWrapper>();
        }
    }
}
