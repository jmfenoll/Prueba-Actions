using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WayToCol.Estate.Service.Public.Repository;
using WayToCol.EstateFile.Service.Public.Repository;

namespace WayToCol.Estate.Service.Public
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
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
