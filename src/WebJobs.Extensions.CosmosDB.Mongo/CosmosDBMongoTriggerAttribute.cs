// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    internal class CosmosDBMongoTriggerAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the database to which the parameter applies.        
        /// May include binding parameters.
        /// </summary>
        [ConnectionString]
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the name of the database to which the parameter applies.        
        /// May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string DatabaseName { get; private set; }

        /// <summary>
        /// Gets the name of the container to which the parameter applies. 
        /// May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string CollectionName { get; private set; }

        /// <summary>
        /// Gets the name of the database to which the parameter applies.        
        /// May include binding parameters.
        /// </summary>
        [ConnectionString]
        public string LeaseConnectionString { get; private set; }

        /// <summary>
        /// Gets the name of the database to which the parameter applies.        
        /// May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string LeaseDatabaseName { get; private set; }

        /// <summary>
        /// Gets the name of the container to which the parameter applies. 
        /// May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string LeaseCollectionName { get; private set; }
    }
}
