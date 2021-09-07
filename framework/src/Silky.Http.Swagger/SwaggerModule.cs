using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Http.Core;

namespace Silky.Http.Swagger
{
    [DependsOn(typeof(SilkyHttpCoreModule))]
    public class SwaggerModule : WebSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerDocuments();

        }
        public override void Configure(IApplicationBuilder application)
        {
            application.UseSwaggerDocuments();
           
        }
    }
}