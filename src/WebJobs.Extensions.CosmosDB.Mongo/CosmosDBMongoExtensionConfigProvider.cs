// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Microsoft.Azure.WebJobs.Extensions.CosmosDB.Mongo
{
    internal class CosmosDBMongoExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly CosmosDBMongoOptions _options;
        private readonly ICosmosDBMongoServiceFactory _serviceFactory;
        private readonly IConfiguration _configuration;

        public CosmosDBMongoExtensionConfigProvider(
            IOptions<CosmosDBMongoOptions> options,
            ICosmosDBMongoServiceFactory serviceFactory,
            IConfiguration configuration)
        {
            this._options = options.Value;
            this._serviceFactory = serviceFactory;
            this._configuration = configuration;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            var triggerRule = context.AddBindingRule<CosmosDBMongoTriggerAttribute>();
            triggerRule.BindToTrigger(new CosmosDBMongoTriggerAttributeBindingProvider(_configuration, _options, this));

            var rule = context.AddBindingRule<CosmosDBMongoAttribute>();
            rule.BindToInput<MongoClient>(attribute => _serviceFactory.CreateService(attribute.ConnectionString));
        }

        internal MongoClient GetService(string connection)
        {
            return _serviceFactory.CreateService(connection);
        }
    }
}
