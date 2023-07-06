using System.Reflection;
using Microsoft.OpenApi.Models;
using SugarTalk.Api.Filters.Swagger;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Swagger;

namespace SugarTalk.Api.Extensions;

public static class SwaggerExtension
{
    public static void AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SugarTalk.Api", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Scheme = "Bearer",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            var basePath = AppContext.BaseDirectory;
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(basePath, xmlFilename), true);
            c.IncludeXmlComments(Path.Combine(basePath, SwaggerDocs.XmlName), true);
            
            c.SchemaFilter<SwaggerShowEnumDescriptionFilter>();
        });
    }
}