using Mediator.Net;
using Mediator.Net.MicrosoftDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SugarTalk.Core;
using SugarTalk.Core.Handlers.CommandHandlers;
using SugarTalk.Core.Middlewares;
using SugarTalk.Core.Services.Kurento;

namespace SugarTalk.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "SugarTalk.Api", Version = "v1"});
            });
            services.RegisterMediator(CreateMediatorBuilder());
            services.LoadSugarTalkModule();
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddSignalR(config =>
            {
                config.EnableDetailedErrors = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SugarTalk.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<RoomHub>("/roomHub");
            });
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private MediatorBuilder CreateMediatorBuilder()
        {
            var mediaBuilder = new MediatorBuilder();
            mediaBuilder.RegisterHandlers(typeof(ScheduleMeetingCommandHandler).Assembly);
            mediaBuilder.ConfigureGlobalReceivePipe(x => x.UseUnifyResponseMiddleware());
            return mediaBuilder;
        }
    }
}