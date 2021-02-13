using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Runtime.Support
{
    public interface IFallbackInvoker : ITransientDependency
    {
        Task Invoke(IDictionary<string, object> parameters);
    }
}