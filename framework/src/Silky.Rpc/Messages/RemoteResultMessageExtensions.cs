using System;
using Newtonsoft.Json.Linq;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Messages
{
    public static class RemoteResultMessageExtensions
    {
        private static IServiceEntryLocator _serviceEntryLocator;
        private static ITypeConvertibleService _typeConvertibleService;

        static RemoteResultMessageExtensions()
        {
            _serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
            _typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
        }

        public static object GetResult(this RemoteResultMessage remoteResultMessage)
        {
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteResultMessage.ServiceId);

            if (remoteResultMessage.Result.GetType() == serviceEntry.ReturnType)
            {
                return remoteResultMessage.Result;
            }
    
            return _typeConvertibleService.Convert(remoteResultMessage.Result,serviceEntry.ReturnType);
        }
    }
}