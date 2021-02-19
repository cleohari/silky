using System;
using System.Linq;
using System.Threading.Tasks;
using Lms.Caching;
using Lms.Core.DependencyInjection;
using Lms.Core.DynamicProxy;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transport.CachingIntercept;

namespace Lms.Rpc.Proxy.Interceptors
{
    public class RpcClientProxyInterceptor : LmsInterceptor, ITransientDependency
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly ICurrentServiceKey _currentServiceKey;
        private readonly IDistributedInterceptCache _distributedCache;

        public RpcClientProxyInterceptor(
            IServiceIdGenerator serviceIdGenerator,
            IServiceEntryLocator serviceEntryLocator,
            ICurrentServiceKey currentServiceKey,
            IDistributedInterceptCache distributedCache)
        {
            _serviceIdGenerator = serviceIdGenerator;
            _serviceEntryLocator = serviceEntryLocator;
            _currentServiceKey = currentServiceKey;
            _distributedCache = distributedCache;
        }

        public async override Task InterceptAsync(ILmsMethodInvocation invocation)
        {
            async Task<object> GetResultFirstFromCache(string cacheName, string cacheKey, ServiceEntry entry)
            {
                _distributedCache.UpdateCacheName(cacheName);
                return await _distributedCache.GetOrAddAsync(cacheKey,
                    invocation.Method.GetReturnType(),
                    async () => await entry.Executor(_currentServiceKey.ServiceKey,
                        invocation.Arguments));
            }

            var servcieId = _serviceIdGenerator.GenerateServiceId(invocation.Method);
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(servcieId);
            try
            {
                if (serviceEntry.GovernanceOptions.CacheEnabled)
                {
                    var removeCachingInterceptProviders = serviceEntry.RemoveCachingInterceptProviders;
                    if (removeCachingInterceptProviders.Any())
                    {
                        foreach (var removeCachingInterceptProvider in removeCachingInterceptProviders)
                        {
                            var removeCacheKey = serviceEntry.GetCachingInterceptKey(invocation.Arguments,
                                removeCachingInterceptProvider.KeyTemplete);
                            _distributedCache.UpdateCacheName(removeCachingInterceptProvider.CacheName);
                            await _distributedCache.RemoveAsync(removeCacheKey, true);
                        }
                    }
                    if (serviceEntry.GetCachingInterceptProvider != null)
                    {
                        var getCacheKey = serviceEntry.GetCachingInterceptKey(invocation.Arguments,
                            serviceEntry.GetCachingInterceptProvider.KeyTemplete);
                        invocation.ReturnValue =
                            await GetResultFirstFromCache(serviceEntry.GetCachingInterceptProvider.CacheName,
                                getCacheKey,
                                serviceEntry);
                    }
                    else if (serviceEntry.UpdateCachingInterceptProvider != null)
                    {
                        var updateCacheKey = serviceEntry.GetCachingInterceptKey(invocation.Arguments,
                            serviceEntry.UpdateCachingInterceptProvider.KeyTemplete);
                        await _distributedCache.RemoveAsync(updateCacheKey);
                        invocation.ReturnValue =
                            await GetResultFirstFromCache(serviceEntry.UpdateCachingInterceptProvider.CacheName,
                                updateCacheKey, serviceEntry);
                    }
                    else
                    {
                        invocation.ReturnValue =
                            await serviceEntry.Executor(_currentServiceKey.ServiceKey, invocation.Arguments);
                    }
                }
                else
                {
                    invocation.ReturnValue =
                        await serviceEntry.Executor(_currentServiceKey.ServiceKey, invocation.Arguments);
                }
            }
            catch (Exception e)
            {
                if (!e.IsBusinessException() && serviceEntry.FallBackExecutor != null)
                {
                    await invocation.ProceedAsync();
                }
                else
                {
                    throw;
                }
            }
        }
    }
}