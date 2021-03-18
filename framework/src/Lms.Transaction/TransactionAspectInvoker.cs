﻿using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Runtime.Server;
using Lms.Transaction.Handler;

namespace Lms.Transaction
{
    public class TransactionAspectInvoker
    {
        private static TransactionAspectInvoker invoker = new();


        private TransactionAspectInvoker()
        {
        }

        public static TransactionAspectInvoker GetInstance()
        {
            return invoker;
        }

        public async Task Invoke(TransactionContext transactionContext, ILmsMethodInvocation invocation)
        {
            var transactionHandlerFactory = EngineContext.Current.Resolve<ITransactionHandlerFactory>();
            if (transactionHandlerFactory != null)
            {
                var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
                var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;

                var transactionHandler =
                    transactionHandlerFactory.FactoryOf(transactionContext, serviceEntry, serviceKey);
                if (transactionHandler != null)
                {
                    await transactionHandler.Handler(transactionContext, invocation);
                }
                else
                {
                    await invocation.ProceedAsync();
                }

            }
            else
            {
                await invocation.ProceedAsync();
            }
        }
    }
}