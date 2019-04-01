using Green.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Green.Web.Framework.Infrastructure.Extensions;
using System;
using Microsoft.Extensions.Configuration;
using NSwag.SwaggerGeneration.Processors.Security;
using NSwag;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNet.OData.Extensions;

namespace Green.Web
{
    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddOData();
            services.AddSwaggerDocument(settings =>
            {
                settings.Description = "Solotralveller web api back end tools";
                settings.Title = "Solotralveller API";
                // Add operation security scope processor
                settings.DocumentName = "Solotralveller API";
                settings.OperationProcessors.Add(new OperationSecurityScopeProcessor("API_KEY"));
                // Add custom document processors, etc.
                settings.DocumentProcessors.Add(new SecurityDefinitionAppender("API_KEY", new SwaggerSecurityScheme
                {
                    Type = SwaggerSecuritySchemeType.ApiKey,
                    Name = "API_KEY",
                    In = SwaggerSecurityApiKeyLocation.Header,
                    Description = "API key authorization"
                }));
                // Post process the generated document
                settings.PostProcess = (document) =>
                {
                    document.Schemes = new List<SwaggerSchema>
                    {
                            SwaggerSchema.Https,SwaggerSchema.Http
                    };
                };
                settings.SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            });
            return services.ConfigureApplicationServices(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
			using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				var context = serviceScope.ServiceProvider.GetRequiredService<GreenObjectContext>();
				context.Database.Migrate();

			}
            app.UseSwagger();
            app.UseSwaggerUi3();
            app.ConfigureRequestPipeline();


        }
    }
}
