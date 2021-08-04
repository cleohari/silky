using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.Rpc.Utils;
using SkyApm.Config;

namespace Silky.Rpc.SkyApm.Configuration
{
    internal static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddSkyWalkingDefaultConfig(this IConfigurationBuilder builder,
            IConfiguration configuration)
        {
            var defaultLogFile = Path.Combine("logs", "skyapm-{Date}.log");
            var defaultConfig = new Dictionary<string, string>
            {
                { "SkyWalking:Namespace", configuration?.GetSection("SkyWalking:Namespace").Value ?? string.Empty },
                { "SkyWalking:ServiceName", configuration?.GetSection("SkyWalking:ServiceName").Value ?? "My_Service" },
                {
                    "Skywalking:ServiceInstanceName",
                    configuration?.GetSection("SkyWalking:ServiceInstanceName").Value ??
                    BuildDefaultServiceInstanceName()
                },
                {
                    "SkyWalking:HeaderVersions:0",
                    configuration?.GetSection("SkyWalking:HeaderVersions:0").Value ?? HeaderVersions.SW8
                },
                {
                    "SkyWalking:Sampling:SamplePer3Secs",
                    configuration?.GetSection("SkyWalking:Sampling:SamplePer3Secs").Value ?? "-1"
                },
                {
                    "SkyWalking:Sampling:Percentage",
                    configuration?.GetSection("SkyWalking:Sampling:Percentage").Value ?? "-1"
                },
                {
                    "SkyWalking:Logging:Level",
                    configuration?.GetSection("SkyWalking:Logging:Level").Value ?? "Information"
                },
                {
                    "SkyWalking:Logging:FilePath",
                    configuration?.GetSection("SkyWalking:Logging:FilePath").Value ?? defaultLogFile
                },
                {
                    "SkyWalking:Transport:Interval",
                    configuration?.GetSection("SkyWalking:Transport:Interval").Value ?? "3000"
                },
                {
                    "SkyWalking:Transport:ProtocolVersion",
                    configuration?.GetSection("SkyWalking:Transport:ProtocolVersion").Value ?? ProtocolVersions.V8
                },
                {
                    "SkyWalking:Transport:QueueSize",
                    configuration?.GetSection("SkyWalking:Transport:QueueSize").Value ?? "30000"
                },
                {
                    "SkyWalking:Transport:BatchSize",
                    configuration?.GetSection("SkyWalking:Transport:BatchSize").Value ?? "3000"
                },
                {
                    "SkyWalking:Transport:gRPC:Servers",
                    configuration?.GetSection("SkyWalking:Transport:gRPC:Servers").Value ?? "localhost:11800"
                },
                {
                    "SkyWalking:Transport:gRPC:Timeout",
                    configuration?.GetSection("SkyWalking:Transport:gRPC:Timeout").Value ?? "10000"
                },
                {
                    "SkyWalking:Transport:gRPC:ReportTimeout",
                    configuration?.GetSection("SkyWalking:Transport:gRPC:ReportTimeout").Value ?? "600000"
                },
                {
                    "SkyWalking:Transport:gRPC:ConnectTimeout",
                    configuration?.GetSection("SkyWalking:Transport:gRPC:ConnectTimeout").Value ?? "10000"
                }
            };
            return builder.AddInMemoryCollection(defaultConfig);
        }

        private static string BuildDefaultServiceInstanceName()
        {
            var moduleContainer = EngineContext.Current.Resolve<IModuleContainer>();
            var modules = moduleContainer.Modules;
            var guid = Guid.NewGuid().ToString("N");
            try
            {
                if (modules.Any(p => p.Name == "DotNettyTcp"))
                {
                    var addressModel = NetUtil.GetRpcAddressModel();
                    var ipAddress = $"{addressModel.Address}:{addressModel.Port}";

                    return $"{ipAddress}";
                }

                var hostAddress = NetUtil.GetLocalAddress();
                var instanceName = $"{hostAddress}@gateway";
                return instanceName;
            }
            catch (Exception)
            {
                return guid;
            }
        }
    }
}