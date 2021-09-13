using Silky.Core.Rpc;
using Silky.SkyApm.Diagnostics.Abstraction.Collections;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace Silky.SkyApm.Diagnostics.Abstraction.Factory
{
    public class SilkySegmentContextFactory : ISilkySegmentContextFactory
    {
        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;

        public SilkySegmentContextFactory(ITracingContext tracingContext,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            ILocalSegmentContextAccessor localSegmentContextAccessor,
            IExitSegmentContextAccessor exitSegmentContextAccessor)
        {
            _tracingContext = tracingContext;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
        }

        public SegmentContext GetEntryContext(string serviceEntryId)
        {
            var context = _entrySegmentContextAccessor.Context;
            if (context == null)
            {
                var carrierHeader = new SilkyCarrierHeaderCollection(RpcContext.Context);
                var serverAddress = RpcContext.Context.GetServerAddress();
                context =
                    _tracingContext.CreateEntrySegmentContext($"[ServerHandle]{serviceEntryId}",
                        carrierHeader);
                context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
                context.Span.Component = SilkyComponents.SilkyRpc;
                context.Span.Peer = serverAddress;
            }

            return context;
        }

        public SegmentContext GetExitSContext(string serviceEntryId)
        {
            var context = _exitSegmentContextAccessor.Context;
            if (context == null)
            {
                var serverAddress = RpcContext.Context.GetServerAddress();
                context = _tracingContext.CreateExitSegmentContext($"[ClientInvoke]{serviceEntryId}",
                    serverAddress, new SilkyCarrierHeaderCollection(RpcContext.Context));

                context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
                context.Span.Component = SilkyComponents.SilkyRpc;
            }

            return context;
        }

        public SegmentContext GetCurrentContext(string operationName)
        {
            var context = _localSegmentContextAccessor.Context;
            if (context == null)
            {
                context = _tracingContext.CreateLocalSegmentContext(operationName);
                context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
                context.Span.Component = SilkyComponents.SilkyTransaction;
            }

            return context;
        }


        public void ReleaseContext(SegmentContext context)
        {
            _tracingContext.Release(context);
        }
    }
}