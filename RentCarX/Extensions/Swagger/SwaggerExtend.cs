using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace RentCarX.Presentation.Extensions.Swagger;

public static class SwaggerExtend
{
    public static void AddSwaggerImplementation(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(c =>
        {
            var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider()
                .GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                c.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = $"Api version: {description.ApiVersion}",
                    Version = description.ApiVersion.ToString()
                });
            }

            c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Provide JWT Token without 'Bearer' prefix"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "bearerAuth"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

    }
}
