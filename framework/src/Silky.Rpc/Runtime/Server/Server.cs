using System.Collections.Generic;
using System.Linq;
using Silky.Core;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Server
{
    public class Server : IServer
    {
        public Server(string hostName)
        {
            HostName = hostName;
            Endpoints = new List<ISilkyEndpoint>();
            Services = new List<ServiceDescriptor>();
        }

        public string HostName { get; }
        public ICollection<ISilkyEndpoint> Endpoints { get; set; }
        public ICollection<ServiceDescriptor> Services { get; set; }

        public void RemoveEndpoint(ISilkyEndpoint endpoint)
        {
            Endpoints = Endpoints.Where(p => !p.Equals(endpoint)).ToList();
            var serverManager = EngineContext.Current.Resolve<IServerManager>();
            serverManager.Update(this);
        }

        public override bool Equals(object? obj)
        {
            var model = obj as Server;
            if (model == null)
            {
                return false;
            }

            
            if (!HostName.Equals(model.HostName))
            {
                return false;
            }

            return
                Services.Count == model.Services.Count
                && Services.All(p => model.Services.Any(p.Equals))
                && Endpoints.Count == model.Endpoints.Count
                && Endpoints.All(p => model.Endpoints.Any(p.Equals));
            
        }

        public static bool operator ==(Server model1, Server model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(Server model1, Server model2)
        {
            return !Equals(model1, model2);
        }

        public override int GetHashCode()
        {
            return (string.Join(",", Services.Select(p => p.ToString())) +
                    string.Join(",", Endpoints.Select(p => p.ToString()))).GetHashCode();
        }
    }
}