// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MongoDB.Driver;

namespace Microsoft.Azure.WebJobs.Extensions.CosmosDB.Mongo
{
    public class MongoCollectionReference
    {
        public MongoCollectionReference(MongoClient client, string databaseName, string collectionName)
        {
            this.Client = client;
            this.DatabaseName = databaseName;
            this.CollectionName = collectionName;
        }

        public MongoClient Client { get; }

        public string DatabaseName { get; }

        public string CollectionName { get; }
    }
}
