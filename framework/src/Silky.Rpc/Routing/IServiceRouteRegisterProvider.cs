using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Routing
{
    public interface IServiceRouteRegisterProvider : ISingletonDependency
    {
        Task RegisterTcpRoutes();
        Task RegisterHttpRoutes();
        Task RegisterWsRoutes(int wsPort);
    }
}