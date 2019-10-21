﻿using System;
using System.Threading.Tasks;

namespace Dotmim.Sync
{

    public static class InterceptorsExtensions
    {

        private static void SetInterceptor<T>(this IOrchestrator<CoreProvider> orchestrator, Func<T, Task> func) where T : ProgressArgs
        {
            orchestrator.Provider.On(new Interceptor<T>(func));
        }

        private static void SetInterceptor<T>(this IOrchestrator<CoreProvider> orchestrator, Action<T> action) where T : ProgressArgs
        {
            orchestrator.Provider.On(new Interceptor<T>(action));
        }

        /// <summary>
        /// Intercept the provider action whenever a connection is opened
        /// </summary>
        public static void OnConnectionOpen(this IOrchestrator<CoreProvider> orchestrator, Func<ConnectionOpenArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action whenever a connection is opened
        /// </summary>
        public static void OnConnectionOpen(this IOrchestrator<CoreProvider> orchestrator, Action<ConnectionOpenArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider action whenever a transaction is opened
        /// </summary>
        public static void OnTransactionOpen(this IOrchestrator<CoreProvider> orchestrator, Action<TransactionOpenArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider action whenever a transaction is opened
        /// </summary>
        public static void OnTransactionOpen(this IOrchestrator<CoreProvider> orchestrator, Func<ConnectionOpenArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action whenever a connection is closed
        /// </summary>
        public static void OnConnectionClose(this IOrchestrator<CoreProvider> orchestrator, Func<ConnectionCloseArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action whenever a connection is closed
        /// </summary>
        public static void OnConnectionClose(this IOrchestrator<CoreProvider> orchestrator, Action<ConnectionCloseArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider action whenever a transaction is commit
        /// </summary>
        public static void OnTransactionCommit(this IOrchestrator<CoreProvider> orchestrator, Action<TransactionCommitArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider action whenever a transaction is commit
        /// </summary>
        public static void OnTransactionCommit(this IOrchestrator<CoreProvider> orchestrator, Func<TransactionCommitArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action when session begin is called
        /// </summary>
        public static void OnOutdated(this IOrchestrator<CoreProvider> orchestrator, Func<OutdatedArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action when session begin is called
        /// </summary>
        public static void OnOutdated(this IOrchestrator<CoreProvider> orchestrator, Action<OutdatedArgs> action)
            => orchestrator.SetInterceptor(action);


        /// <summary>
        /// Intercept the provider when an apply change is failing
        /// </summary>
        public static void OnApplyChangesFailed(this IOrchestrator<CoreProvider> orchestrator, Func<ApplyChangesFailedArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider when an apply change is failing
        /// </summary>
        public static void OnApplyChangesFailed(this IOrchestrator<CoreProvider> orchestrator, Action<ApplyChangesFailedArgs> action)
            => orchestrator.SetInterceptor(action);


        /// <summary>
        /// Intercept the provider action when session begin is called
        /// </summary>
        public static void OnSessionBegin(this IOrchestrator<CoreProvider> orchestrator, Func<SessionBeginArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action when session begin is called
        /// </summary>
        public static void OnSessionBegin(this IOrchestrator<CoreProvider> orchestrator, Action<SessionBeginArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider action when session end is called
        /// </summary>
        public static void OnSessionEnd(this IOrchestrator<CoreProvider> orchestrator, Func<SessionEndArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action when session end is called
        /// </summary>
        public static void OnSessionEnd(this IOrchestrator<CoreProvider> orchestrator, Action<SessionEndArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider when schema is readed
        /// </summary>
        public static void OnSchema(this IOrchestrator<CoreProvider> orchestrator, Func<SchemaArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider when schema is readed
        /// </summary>
        public static void OnSchema(this IOrchestrator<CoreProvider> orchestrator, Action<SchemaArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider before it begins a database deprovisioning
        /// </summary>
        public static void OnDatabaseDeprovisioning(this IOrchestrator<CoreProvider> orchestrator, Func<DatabaseDeprovisioningArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider before it begins a database deprovisioning
        /// </summary>
        public static void OnDatabaseDeprovisioning(this IOrchestrator<CoreProvider> orchestrator, Action<DatabaseDeprovisioningArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider after it has deprovisioned a database
        /// </summary>
        public static void OnDatabaseDeprovisioned(this IOrchestrator<CoreProvider> orchestrator, Func<DatabaseDeprovisionedArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider after it has deprovisioned a database
        /// </summary>
        public static void OnDatabaseDeprovisioned(this IOrchestrator<CoreProvider> orchestrator, Action<DatabaseDeprovisionedArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider before it begins a table deprovisioning
        /// </summary>
        public static void OnTabeDeprovisioning(this IOrchestrator<CoreProvider> orchestrator, Func<TableDeprovisioningArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider before it begins a table deprovisioning
        /// </summary>
        public static void OnTabeDeprovisioning(this IOrchestrator<CoreProvider> orchestrator, Action<TableDeprovisioningArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider after it has deprovisioned a table
        /// </summary>
        public static void OnTabledDeprovisioned(this IOrchestrator<CoreProvider> orchestrator, Func<TableDeprovisionedArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider after it has deprovisioned a table
        /// </summary>
        public static void OnTabledDeprovisioned(this IOrchestrator<CoreProvider> orchestrator, Action<TableDeprovisionedArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider before it begins a database provisioning
        /// </summary>
        public static void OnDatabaseProvisioning(this IOrchestrator<CoreProvider> orchestrator, Func<DatabaseProvisioningArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider before it begins a database provisioning
        /// </summary>
        public static void OnDatabaseProvisioning(this IOrchestrator<CoreProvider> orchestrator, Action<DatabaseProvisioningArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider after it has provisioned a database
        /// </summary>
        public static void OnDatabaseProvisioned(this IOrchestrator<CoreProvider> orchestrator, Func<DatabaseProvisionedArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider after it has provisioned a database
        /// </summary>
        public static void OnDatabaseProvisioned(this IOrchestrator<CoreProvider> orchestrator, Action<DatabaseProvisionedArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider before it begins a table provisioning
        /// </summary>
        public static void OnTabeProvisioning(this IOrchestrator<CoreProvider> orchestrator, Func<TableDeprovisioningArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider before it begins a table provisioning
        /// </summary>
        public static void OnTabeProvisioning(this IOrchestrator<CoreProvider> orchestrator, Action<TableDeprovisioningArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider after it has provisioned a table
        /// </summary>
        public static void OnTabledProvisioned(this IOrchestrator<CoreProvider> orchestrator, Func<TableProvisionedArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider after it has provisioned a table
        /// </summary>
        public static void OnTabledProvisioned(this IOrchestrator<CoreProvider> orchestrator, Action<TableProvisionedArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider action when changes are going to be selected on each table defined in the configuration schema
        /// </summary>
        public static void OnTableChangesSelecting(this IOrchestrator<CoreProvider> orchestrator, Func<TableChangesSelectingArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action when changes are going to be selected on each table defined in the configuration schema
        /// </summary>
        public static void OnTableChangesSelecting(this IOrchestrator<CoreProvider> orchestrator, Action<TableChangesSelectingArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider action when changes are selected on each table defined in the configuration schema
        /// </summary>
        public static void OnTableChangesSelected(this IOrchestrator<CoreProvider> orchestrator, Func<TableChangesSelectedArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action when changes are selected on each table defined in the configuration schema
        /// </summary>
        public static void OnTableChangesSelected(this IOrchestrator<CoreProvider> orchestrator, Action<TableChangesSelectedArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider action when changes are going to be applied on each table defined in the configuration schema
        /// </summary>
        public static void OnTableChangesApplying(this IOrchestrator<CoreProvider> orchestrator, Func<TableChangesApplyingArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action when changes are going to be applied on each table defined in the configuration schema
        /// </summary>
        public static void OnTableChangesApplying(this IOrchestrator<CoreProvider> orchestrator, Action<TableChangesApplyingArgs> action)
            => orchestrator.SetInterceptor(action);

        /// <summary>
        /// Intercept the provider action when changes are applied on each table defined in the configuration schema
        /// </summary>
        public static void OnTableChangesApplied(this IOrchestrator<CoreProvider> orchestrator, Func<TableChangesAppliedArgs, Task> func)
            => orchestrator.SetInterceptor(func);

        /// <summary>
        /// Intercept the provider action when changes are applied on each table defined in the configuration schema
        /// </summary>
        public static void OnTableChangesApplied(this IOrchestrator<CoreProvider> orchestrator, Action<TableChangesAppliedArgs> action)
            => orchestrator.SetInterceptor(action);

    }
}
