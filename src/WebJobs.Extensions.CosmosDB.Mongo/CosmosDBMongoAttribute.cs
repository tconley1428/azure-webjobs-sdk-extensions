// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class CosmosDBMongoAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="databaseName">The Azure Cosmos database name.</param>
        /// <param name="collectionName">The Azure Cosmos container name.</param>
        public CosmosDBMongoAttribute(string connectionString, string databaseName, string collectionName)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            CollectionName = collectionName;
        }

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
        public string ConnectionString { get; private set; }
    }
}
