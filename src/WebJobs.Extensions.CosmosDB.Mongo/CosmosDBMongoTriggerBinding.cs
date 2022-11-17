// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using MongoDB.Bson;

namespace Microsoft.Azure.WebJobs.Extensions.CosmosDB.Mongo
{
    internal class CosmosDBMongoTriggerBinding : ITriggerBinding    
    {
        private readonly MongoCollectionReference monitoredCollection;
        private readonly MongoCollectionReference leaseCollection;

        public CosmosDBMongoTriggerBinding(MongoCollectionReference monitoredCollection, MongoCollectionReference leaseCollection)
        {
            this.monitoredCollection = monitoredCollection;
            this.leaseCollection = leaseCollection;
        }

        public Type TriggerValueType => typeof(BsonDocument);

        public IReadOnlyDictionary<string, Type> BindingDataContract => throw new NotImplementedException();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            throw new NotImplementedException();
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            return Task.FromResult<IListener>(new MongoTriggerListener(
                context.Executor,
                monitoredCollection,
                leaseCollection));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            throw new NotImplementedException();
        }
    }
}
