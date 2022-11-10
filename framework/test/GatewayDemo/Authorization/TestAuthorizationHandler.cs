using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Silky.Http.Identity.Authorization.Handlers;
using Silky.Rpc.Extensions;

namespace GatewayDemo.Authorization
{
    public class TestAuthorizationHandler : SilkyAuthorizationHandlerBase
    {
        private readonly ILogger<TestAuthorizationHandler> _logger;

        public TestAuthorizationHandler(ILogger<TestAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected async override Task<bool> PipelineAsync(AuthorizationHandlerContext context, HttpContext httpContext)
        {
            var serviceEntryDescriptor = httpContext.GetServiceEntryDescriptor();
            // if (serviceEntry.Services.Id.Contains("ITestApplication"))
            // {
            //     _logger.LogInformation($"{serviceEntry.Id} has permission");
            //     return true;
            // }

            return true;
        }
    }
}