using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using WayToCol.Estate.Service.Public.Repository;

namespace WayToCol.Estate.Service.Public.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class SetupExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddContracts(this IServiceCollection services)
        {
            services.AddTransient<IEstatePublicRepository, EstatePublicMongoDbRepository>();
            services.AddTransient<IEstateFilePublicRepository, EstateFilePublicMongoDbRepository>();
            services.AddLogging(configure => configure.AddSerilog());
            return services;
        }


    }
}
