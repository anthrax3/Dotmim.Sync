﻿using Dotmim.Sync.Batch;
using Dotmim.Sync.Data;
using Dotmim.Sync.Data.Surrogate;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.Filter;
using Dotmim.Sync.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dotmim.Sync
{

    /// <summary>
    /// Sync agent. It's the sync orchestrator
    /// Knows both the Sync Server provider and the Sync Client provider
    /// </summary>
    public class SyncAgent : IDisposable
    {
        private IProgress<ProgressArgs> remoteProgress = null;

        /// <summary>
        /// Defines the state that a synchronization session is in.
        /// </summary>
        public SyncSessionState SessionState { get; set; }

        /// <summary>
        /// Gets or Sets the local orchestrator
        /// </summary>
        public ILocalOrchestrator LocalOrchestrator { get; set; }

        /// <summary>
        /// Get or Sets the remote orchestrator
        /// </summary>
        public IRemoteOrchestrator RemoteOrchestrator { get; set; }

        /// <summary>
        /// Get or Sets the Sync parameter to pass to Remote provider for filtering rows
        /// </summary>
        public SyncParameterCollection Parameters { get; private set; }

        /// <summary>
        /// Occurs when sync is starting, ending
        /// </summary>
        public event EventHandler<SyncSessionState> SessionStateChanged = null;

        /// <summary>
        /// Gets or Sets the schema used for this sync process.
        /// </summary>
        public SyncSchema Schema { get; set; }

        /// <summary>
        /// Gets or Sets the options used on this sync process.
        /// </summary>
        public SyncOptions Options { get; set; }

        public void SetInterceptors(Interceptors interceptors)
            => this.LocalOrchestrator.On(interceptors);

        /// <summary>
        /// If you want to see remote progress as well (only available RemoteOrchestrator)
        /// </summary>
        /// <param name="remoteProgress"></param>
        public void AddRemoteProgress(IProgress<ProgressArgs> remoteProgress) => this.remoteProgress = remoteProgress;

        /// <summary>
        /// Shortcut to Apply changed failed (supported by the local orchestrator)
        /// </summary>
        public void OnApplyChangesFailed(Func<ApplyChangesFailedArgs, Task> func) => this.LocalOrchestrator.OnApplyChangesFailed(func);

        /// <summary>
        /// Shortcut to Apply changed failed (supported by the local orchestrator)
        /// </summary>
        public void OnApplyChangesFailed(Action<ApplyChangesFailedArgs> action) => this.LocalOrchestrator.OnApplyChangesFailed(action);


        public SyncAgent(string scopeName, CoreProvider clientProvider, CoreProvider serverProvider, string[] tables = null, SyncOptions options = null)
            : this(scopeName, new LocalOrchestrator(clientProvider), new RemoteOrchestrator(serverProvider), tables, options)
        {
        }
        public SyncAgent(CoreProvider clientProvider, CoreProvider serverProvider, string[] tables = null, SyncOptions options = null)
            : this(SyncOptions.DefaultScopeName, clientProvider, serverProvider, tables, options)
        {
        }
        public SyncAgent(string scopeName, CoreProvider clientProvider, IRemoteOrchestrator remoteOrchestrator, string[] tables = null, SyncOptions options = null)
            : this(scopeName, new LocalOrchestrator(clientProvider), remoteOrchestrator, tables, options)
        {
        }
        public SyncAgent(CoreProvider clientProvider, IRemoteOrchestrator remoteOrchestrator, string[] tables = null, SyncOptions options = null)
            : this(SyncOptions.DefaultScopeName, new LocalOrchestrator(clientProvider), remoteOrchestrator, tables, options)
        {
        }
        public SyncAgent(ILocalOrchestrator localOrchestrator, IRemoteOrchestrator remoteOrchestrator, string[] tables = null, SyncOptions options = null)
            : this(SyncOptions.DefaultScopeName, localOrchestrator, remoteOrchestrator, tables, options)
        {
        }

        public SyncAgent(string scopeName, ILocalOrchestrator localOrchestrator, IRemoteOrchestrator remoteOrchestrator, string[] tables = null, SyncOptions options = null)
        {
            if (string.IsNullOrEmpty(scopeName))
                throw new ArgumentNullException("scopeName");

            if (remoteOrchestrator.Provider != null && !remoteOrchestrator.Provider.CanBeServerProvider)
                throw new NotSupportedException();

            // Create schema based on list of tables
            this.Schema = new SyncSchema(tables) { ScopeName = scopeName };

            // Create sync options if needed
            this.Options = options ?? new SyncOptions();

            // Add parameters
            this.Parameters = new SyncParameterCollection();

            // Affect local and remote orchestrators
            this.LocalOrchestrator = localOrchestrator;
            this.RemoteOrchestrator = remoteOrchestrator;
        }


        /// <summary>
        /// Launch a normal synchronization without any IProgess or CancellationToken
        /// </summary>
        public Task<SyncContext> SynchronizeAsync() => SynchronizeAsync(SyncType.Normal, CancellationToken.None);

        /// <summary>
        /// Launch a normal synchronization without any IProgess or CancellationToken
        /// </summary>
        public Task<SyncContext> SynchronizeAsync(IProgress<ProgressArgs> progress) => SynchronizeAsync(SyncType.Normal, CancellationToken.None, progress);

        /// <summary>
        /// Launch a synchronization with the specified mode
        /// </summary>
        public async Task<SyncContext> SynchronizeAsync(SyncType syncType, CancellationToken cancellationToken, IProgress<ProgressArgs> progress = null)
        {
            // Context, used to back and forth data between servers
            var context = new SyncContext(Guid.NewGuid())
            {
                // set start time
                StartTime = DateTime.Now,
                // if any parameters, set in context
                Parameters = this.Parameters,
                // set sync type (Normal, Reinitialize, ReinitializeWithUpload)
                SyncType = syncType
            };

            this.SessionState = SyncSessionState.Synchronizing;
            this.SessionStateChanged?.Invoke(this, this.SessionState);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                ScopeInfo scope = null;

                // Starts sync by :
                // - Getting local config we have set by code
                // - Ensure local scope is created (table and values)
                (context, scope) = await this.LocalOrchestrator.EnsureScopeAsync
                        (context, this.Schema, this.Options, cancellationToken, progress);

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                // check if we already have a schema from local 
                if (!string.IsNullOrEmpty(scope.Schema))
                {
                    this.Schema = JsonConvert.DeserializeObject<SyncSchema>(scope.Schema);
                    this.Schema.SetLight.ReadSchemaIntoDmSet(this.Schema.GetSet());
                }
                else
                {
                    // FIRST call to server
                    // Get the server scope info and server reference id to local scope
                    // Be sure options / schema from client are passed if needed
                    // Then the configuration with full schema
                    var serverSchema = await this.RemoteOrchestrator.EnsureSchemaAsync(
                            context, this.Schema, cancellationToken, remoteProgress);
                    context = serverSchema.context;
                    this.Schema = serverSchema.schema;
                }


                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                // on local orchestrator, get local changes
                // Most probably the schema has changed, so we passed it again (coming from Server)
                // Don't need to pass again Options since we are not modifying it between server and client
                var clientChanges = await this.LocalOrchestrator.GetChangesAsync(
                    context, this.Schema, scope, this.Options.BatchSize, this.Options.BatchDirectory, cancellationToken, progress);

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                // set context
                context = clientChanges.context;

                // SECOND call to server
                var serverChanges = await this.RemoteOrchestrator.ApplyThenGetChangesAsync(
                    context, scope, this.Schema, clientChanges.clientBatchInfo, this.Options.DisableConstraintsOnApplyChanges, this.Options.UseBulkOperations,
                    this.Options.CleanMetadatas, this.Options.BatchSize, this.Options.BatchDirectory, cancellationToken, remoteProgress);

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                // set context
                context = serverChanges.context;
                // Serialize schema to be able to save it in client db
                if (string.IsNullOrEmpty(scope.Schema))
                {
                    this.Schema.SetLight = new DmSetLightSchema(this.Schema.GetSet());
                    var schemaLight = JsonConvert.SerializeObject(this.Schema);
                    scope.Schema = schemaLight;
                }

                var clientPolicy = this.Schema.ConflictResolutionPolicy == ConflictResolutionPolicy.ServerWins ? ConflictResolutionPolicy.ClientWins : ConflictResolutionPolicy.ServerWins;

                var localChanges = await this.LocalOrchestrator.ApplyChangesAsync(
                    context, scope, this.Schema, serverChanges.serverBatchInfo,
                    clientPolicy, clientChanges.clientTimestamp, serverChanges.remoteClientTimestamp,
                    this.Options.DisableConstraintsOnApplyChanges, this.Options.UseBulkOperations,
                    this.Options.CleanMetadatas, this.Options.ScopeInfoTableName,
                    cancellationToken, progress);

                context.TotalChangesDownloaded = localChanges.clientChangesApplied.TotalAppliedChanges;
                context.TotalChangesUploaded = clientChanges.clientChangesSelected.TotalChangesSelected;
                context.TotalSyncErrors = localChanges.clientChangesApplied.TotalAppliedChangesFailed;

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

            }
            catch (SyncException se)
            {
                Console.WriteLine($"Sync Exception: {se.Message}. Type:{se.Type}.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknwon Exception: {ex.Message}.");
                throw new SyncException(ex, SyncStage.None);
            }
            finally
            {
                // End the current session
                this.SessionState = SyncSessionState.Ready;
                this.SessionStateChanged?.Invoke(this, this.SessionState);
            }

            return context;
        }


        // --------------------------------------------------------------------
        // Dispose
        // --------------------------------------------------------------------

        /// <summary>
        /// Releases all resources used by the <see cref="T:Microsoft.Synchronization.Data.DbSyncBatchInfo" />.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used 
        /// </summary>
        protected virtual void Dispose(bool cleanup)
        {

        }
    }
}