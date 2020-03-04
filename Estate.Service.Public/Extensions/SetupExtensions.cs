using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using WayToCol.Agent.Service.Public.Repository;
using WayToCol.Estate.Service.Public.Domain;
using WayToCol.Estate.Service.Public.Repository;
using WayToCol.EstatStakeholder.Service.Public.Repository;

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
            services.AddTransient<AgentPublicMongoDbRepository, AgentPublicMongoDbRepository>();
            services.AddTransient<EstateStakeholderPublicMongoDbRepository, EstateStakeholderPublicMongoDbRepository>();
            services.AddTransient<EstateDomain, EstateDomain>();
            services.AddLogging(configure => configure.AddSerilog());
            return services;
        }


    }
}
