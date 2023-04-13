using System.Text.Json;
using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using SugarTalk.Api.Extensions;
using SugarTalk.Api.Filters;
using SugarTalk.Api.Middlewares;
using SugarTalk.Api.Middlewares.Authentication;
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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "SugarTalk.Api", Version = "v1"});
            });

            services.AddHttpClientInternal();
            services.AddHttpContextAccessor();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<GoogleAuthenticationOptions, GoogleAuthenticationHandler>("Google", _ => { })
                .AddScheme<WechatAuthenticationOptions, WechatAuthenticationHandler>("Wechat", _ => { })
                .AddScheme<FacebookAuthenticationOptions, FacebookAuthenticationHandler>("Facebook", _ => { });
            
            services.AddAuthorization(options =>
            {
                var builder = new AuthorizationPolicyBuilder("Google", "Wechat", "Facebook");
                builder = builder.RequireAuthenticatedUser();
                options.DefaultPolicy = builder.Build();
            });

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
            app.UseMiddleware<EnrichAccessTokenMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // endpoints.MapHealthChecks("health");
                endpoints.MapHub<MeetingHub>("/meetingHub");
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}