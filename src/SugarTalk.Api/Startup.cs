using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SugarTalk.Api.Middlewares;
using SugarTalk.Api.Middlewares.Authentication;
using SugarTalk.Core;
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

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<GoogleAuthenticationOptions, GoogleAuthenticationHandler>("Google", option =>
                {

                });
            
            services.AddAuthorization(options =>
            {
                var builder = new AuthorizationPolicyBuilder("Google");
                builder = builder.RequireAuthenticatedUser();
                options.DefaultPolicy = builder.Build();
            });
            
            services
                .LoadSugarTalkModule()
                .LoadSettings(Configuration)
                .LoadMongoDb()
                .LoadMediator()
                .LoadServices();
            
            services.AddMvc(options => options.EnableEndpointRouting = false);
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

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            
            app.UseRouting();

            app.UseMiddleware<EnrichAccessTokenMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MeetingHub>("/meetingHub");
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}