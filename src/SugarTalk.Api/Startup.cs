using System.Text.Json;
using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using OpenAI.Extensions;
using Serilog;
using SugarTalk.Api.Extensions;
using SugarTalk.Api.Filters;
using SugarTalk.Core.Hubs;
using SugarTalk.Core.Settings.OpenAi;
using SugarTalk.Messages;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace SugarTalk.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelate(options => options.RequestHeaders = SugarTalkConstants.CorrelationIdHeaders);
            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpClientInternal();
            services.AddMemoryCache();
            services.AddResponseCaching();
            services.AddHealthChecks();
            services.AddEndpointsApiExplorer();
            services.AddHttpContextAccessor();
            services.AddCustomSwagger();
            services.AddCustomAuthentication(Configuration);
            services.AddCorsPolicy(Configuration);

            services.AddMvc(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });
            
            services.AddHangfireInternal(Configuration);

            services.AddSignalR(config =>
            {
                config.EnableDetailedErrors = true;
            }).AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
            
            services.AddOpenAIService(settings => { settings.ApiKey = new OpenAiSettings(Configuration).ApiKey; })
                .ConfigureHttpClient(http => http.Timeout = TimeSpan.FromSeconds(6000));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SugarTalk.Api.xml");
                    c.DocExpansion(DocExpansion.None);
                });
            }
            
            app.UseSerilogRequestLogging();
            app.UseCorrelate();
            app.UseRouting();
            app.UseCors();
            app.UseResponseCaching();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHangfireInternal();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("health");
                endpoints.MapHub<MeetingHub>("/meetingHub");
            });
        }
    }
}