﻿using Dotmim.Sync.Enumerations;
using System;
using System.Runtime.Serialization;

namespace Dotmim.Sync
{
    /// <summary>
    /// Mapping sur la table ScopeInfo
    /// </summary>
    [DataContract(Name = "scope"), Serializable]
    public class ScopeInfo
    {
        /// <summary>
        /// Scope name. Shared by all clients and the server
        /// </summary>
        [DataMember(Name = "n", IsRequired = true, Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Id of the scope owner
        /// </summary>
        [DataMember(Name = "id", IsRequired = true, Order = 2)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or Sets if the current provider is newly created one in database.
        /// If new, we will override timestamp for first synchronisation to be sure to get all datas from server
        /// </summary>
        [DataMember(Name = "in", IsRequired = true, Order = 3)]
        public bool IsNewScope { get; set; }

        /// <summary>
        /// Scope schema. stored locally on the client
        /// </summary>
        [IgnoreDataMember]
        public string Schema { get; set; }

        /// <summary>
        /// Gets or Sets the last datetime when a sync has successfully ended.
        /// </summary>
        [IgnoreDataMember]
        public DateTime? LastSync { get; set; }

        /// <summary>
        /// Gets or Sets the last timestamp a sync has occured. This timestamp is set just 'before' sync start.
        /// </summary>
        [DataMember(Name = "lst", IsRequired = false, EmitDefaultValue = false, Order = 4)]
        public long LastSyncTimestamp { get; set; }

        /// <summary>
        /// Gets or Sets the last server timestamp a sync has occured for this scope client.
        /// </summary>
        [DataMember(Name = "lsst", IsRequired = false, EmitDefaultValue = false, Order = 5)]
        public long LastServerSyncTimestamp { get; set; }

        /// <summary>
        /// Gets or Sets the last duration a sync has occured. 
        /// </summary>
        [IgnoreDataMember]
        public long LastSyncDuration { get; set; }

        /// <summary>
        /// Gets or sets the last time we apply a clean up on metadata
        /// </summary>
        [IgnoreDataMember]
        public long LastCleanupTimestamp { get; set; }

    }
}
