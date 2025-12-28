using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StreamVault.Api.Configuration;

public static class ApiVersioningConfig
{
    public static IServiceCollection ConfigureApiVersioning(this IServiceCollection services)
    {
        // Add API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-Version"),
                new MediaTypeApiVersionReader("v"));
            
            // Report API versions in response headers
            options.ReportApiVersions = true;
            
            // Enable API versioning in route constraints
            options.Conventions.Controller<Controllers.V1.VideoController>()
                   .HasApiVersion(new ApiVersion(1, 0))
                   .HasDeprecatedApiVersion(new ApiVersion(0, 9))
                   .AdvertisesApiVersion(new ApiVersion(1, 0))
                   .AdvertisesApiVersion(new ApiVersion(2, 0));
        });

        // Add API version explorer for Swagger
        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        return services;
    }

    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Include XML Comments
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            // Configure Swagger for multiple API versions
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "StreamVault API",
                Version = "v1.0",
                Description = "StreamVault Video Platform API - Version 1.0",
                Contact = new OpenApiContact
                {
                    Name = "StreamVault Support",
                    Email = "support@streamvault.com",
                    Url = new Uri("https://streamvault.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "StreamVault API",
                Version = "v2.0",
                Description = "StreamVault Video Platform API - Version 2.0 (Latest)",
                Contact = new OpenApiContact
                {
                    Name = "StreamVault Support",
                    Email = "support@streamvault.com",
                    Url = new Uri("https://streamvault.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add security definition for JWT Bearer
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Add API Key security
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key Authentication. Example: \"X-Api-Key: your-api-key\"",
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Custom operation filters
            options.OperationFilter<ApiVersionOperationFilter>();
            options.OperationFilter<SwaggerDefaultValues>();
            options.DocumentFilter<LowercaseDocumentFilter>();
            
            // Tag descriptions
            options.TagActionsBy(apiDesc => apiDesc.GroupName);
            
            // Order tags
            options.OrderActionsBy((apiDesc) => $"{apiDesc.GroupName}_{apiDesc.HttpMethod}_{apiDesc.RelativePath}");
        });

        return services;
    }
}

// Custom filters for Swagger
public class ApiVersionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiVersion = context.ApiDescription.GetApiVersion();
        var versionParameter = new OpenApiParameter
        {
            Name = "api-version",
            In = ParameterLocation.Query,
            Description = "API Version",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString(apiVersion?.ToString() ?? "1.0")
            }
        };

        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(versionParameter);
    }
}

public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;
        operation.Deprecated |= apiDescription.IsDeprecated();

        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                if (responseType.ApiResponseFormats.All(x => x.MediaType != contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }

        if (operation.Parameters == null)
            return;

        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Name == parameter.Name);

            if (description == null)
                continue;

            if (parameter.Description == null)
                parameter.Description = description.ModelMetadata.Description;

            if (parameter.Schema.Default == null && description.DefaultValue != null)
            {
                parameter.Schema.Default = new Microsoft.OpenApi.Any.OpenApiString(description.DefaultValue.ToString());
            }

            parameter.Required |= description.IsRequired;
        }
    }
}

public class LowercaseDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.Keys.ToList();
        foreach (var path in paths)
        {
            var lowerPath = path.ToLowerInvariant();
            if (lowerPath != path)
            {
                swaggerDoc.Paths[lowerPath] = swaggerDoc.Paths[path];
                swaggerDoc.Paths.Remove(path);
            }
        }
    }
}
