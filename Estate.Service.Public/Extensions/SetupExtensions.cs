using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using WayToCol.Agent.Service.Public.Repository;
using WayToCol.Common.Contracts.Estates;
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
            services.AddTransient<IEstateRepository, EstateMongoDbRepository>();
            services.AddTransient<IEstateFileRepository, EstateFileMongoDbRepository>();
            services.AddTransient<AgentMongoDbRepository, AgentMongoDbRepository>();
            services.AddTransient<EstateStakeholderMongoDbRepository, EstateStakeholderMongoDbRepository>();
            services.AddTransient<EstateDomain, EstateDomain>();
            services.AddLogging(configure => configure.AddSerilog());
            return services;
        }

        public static string addTxt(this string texto, string nuevoTexto)
        {
            texto += " " + nuevoTexto;
            texto = texto.Trim();

            return texto;
        }

        public static string addBol(this string texto, EstateDto objeto)
        {
            var props = objeto.GetType().GetProperties();

            var propsWith1 = props.Select(x => new { name = x.Name, value = x.GetValue(objeto) })
                .Where(x => x.value is bool && (bool)x.value == true)
                .Select(x => x.name).ToList();

            if (propsWith1.Count() > 0)
                texto = texto + " " + string.Join(" ", propsWith1);
            return texto;

        }
    }
}
