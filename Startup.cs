using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Swashbuckle.AspNetCore.Swagger;
using TacitCoreDemo.Services;

namespace TacitCoreDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(String.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //app settings configuration
            services.AddSingleton<IConfiguration>(Configuration);
            //Exception filter
            services.AddMvc(options => options.Filters.Add(new CustomErrorFilter())).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //Api Version configuration
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });
            //swagger configuration
            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new Info { Title = "Tacit API", Version = "v1" });                
            }
            );
            services.AddScoped<IWebOrderingService, WebOrderingService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tacit API V1"));
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
