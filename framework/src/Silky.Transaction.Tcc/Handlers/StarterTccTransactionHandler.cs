﻿using System;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Handler;
using Silky.Transaction.Repository.Spi;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class StarterTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            try
            {
                var preTryInfo = await executor.PreTry(invocation);
                var transaction = preTryInfo.Item1;
                var transactionContext = preTryInfo.Item2;
                SilkyTransactionHolder.Instance.Set(transaction);
                SilkyTransactionContextHolder.Set(transactionContext);
                try
                {
                    await invocation.ProceedAsync();
                    transaction.Status = ActionStage.Trying;
                    await executor.UpdateStartStatus(transaction);
                }
                catch (Exception e)
                {
                    var errorCurrentTransaction = SilkyTransactionHolder.Instance.CurrentTransaction;
                    await executor.GlobalCancel(errorCurrentTransaction);
                    throw;
                }

                var currentTransaction = SilkyTransactionHolder.Instance.CurrentTransaction;
                await executor.GlobalConfirm(currentTransaction);
            }
            finally
            {
                SilkyTransactionContextHolder.Remove();
                executor.Remove();
            }
        }
    }
}