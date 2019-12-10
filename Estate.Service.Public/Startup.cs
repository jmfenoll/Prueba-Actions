using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using WayToCol.Common.Api.Swagger;
using WayToCol.Common.Contracts.Estates;

namespace WayToCol.Estate.Service.Public
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        private string _projectName = Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration _configuration { get; }

        /// This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //Swagger en Core 3.0
            //https://docs.microsoft.com/es-es/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-3.0&tabs=visual-studio

            services.AddSwagger(new Info
            {
                Title = "Estate service Public",
                Version = "v1",
                Description = "",
                TermsOfService = "None"
            }, _projectName);

            services.AddContracts();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseMvc();

            app.UsePublicSwagger();

            ConfigureMappings();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ConfigureMappings()
        {

        }
    }
}
