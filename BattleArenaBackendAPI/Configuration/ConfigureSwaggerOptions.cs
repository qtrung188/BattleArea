using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BattleArenaBackendAPI.Configuration
{
    /// <summary>
    /// Generates one Swagger document per discovered API version. Driven by the
    /// Asp.Versioning API explorer, so new versions automatically appear in the
    /// Swagger UI without touching Program.cs.
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfo(description));
            }
        }

        private static OpenApiInfo CreateInfo(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = "BattleArena Backend API",
                Version = description.ApiVersion.ToString()
            };

            if (description.IsDeprecated)
            {
                info.Description = "This API version has been deprecated.";
            }

            return info;
        }
    }
}
