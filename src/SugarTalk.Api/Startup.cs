using System.Text.Json;
using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using SugarTalk.Api.Authentication;
using SugarTalk.Api.Extensions;
using SugarTalk.Api.Filters;
using SugarTalk.Core.Hubs;
using SugarTalk.Messages;

namespace SugarTalk.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelate(options => options.RequestHeaders = SugarTalkConstants.CorrelationIdHeaders);
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "SugarTalk.Api", Version = "v1"});
            });

            services.AddHttpClientInternal();
            services.AddMemoryCache();
            services.AddResponseCaching();
            services.AddHealthChecks();
            services.AddEndpointsApiExplorer();
            services.AddHttpContextAccessor();
            services.AddCustomAuthentication(Configuration);

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add<GlobalExceptionFilter>();
            });
            services.AddSignalR(config =>
            {
                config.EnableDetailedErrors = true;
            }).AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
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
            
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCorrelate();
            app.UseRouting();
            app.UseCors();
            app.UseResponseCaching();
            app.UseMiddleware<EnrichAccessTokenMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("health");
                endpoints.MapHub<MeetingHub>("/meetingHub");
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}