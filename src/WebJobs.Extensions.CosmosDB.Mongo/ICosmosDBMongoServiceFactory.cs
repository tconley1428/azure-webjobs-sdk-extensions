// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MongoDB.Driver;

namespace Microsoft.Azure.WebJobs.Extensions.CosmosDB.Mongo
{
    public interface ICosmosDBMongoServiceFactory
    {
        MongoClient CreateService(string connection);
    }
}