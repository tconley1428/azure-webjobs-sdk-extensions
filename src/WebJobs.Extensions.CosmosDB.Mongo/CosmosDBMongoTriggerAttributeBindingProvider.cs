// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.CosmosDB.Mongo
{
    internal class CosmosDBMongoTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration _configuration;
        private readonly CosmosDBMongoOptions _options;
        private readonly CosmosDBMongoExtensionConfigProvider _configProvider;

        public CosmosDBMongoTriggerAttributeBindingProvider(IConfiguration configuration, CosmosDBMongoOptions options, CosmosDBMongoExtensionConfigProvider configProvider)
        {
            this._configuration = configuration;
            this._options = options;
            this._configProvider = configProvider;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            var attribute = context.Parameter.GetCustomAttribute<CosmosDBMongoTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            return Task.FromResult((ITriggerBinding)new CosmosDBMongoTriggerBinding(
                new MongoCollectionReference(
                    this._configProvider.GetService(ResolveConfigurationValue(attribute.ConnectionString, nameof(attribute.ConnectionString))),
                    ResolveConfigurationValue(attribute.DatabaseName, nameof(attribute.DatabaseName)),
                    ResolveConfigurationValue(attribute.CollectionName, nameof(attribute.CollectionName))),
                new MongoCollectionReference(
                    this._configProvider.GetService(ResolveConfigurationValue(attribute.LeaseConnectionString, nameof(attribute.LeaseConnectionString))),
                    ResolveConfigurationValue(attribute.LeaseDatabaseName, nameof(attribute.LeaseDatabaseName)),
                    ResolveConfigurationValue(attribute.LeaseCollectionName, nameof(attribute.LeaseCollectionName)))));
        }

        internal string ResolveConfigurationValue(string unresolvedConnectionString, string propertyName)
        {
            // First, resolve the string.
            if (!string.IsNullOrEmpty(unresolvedConnectionString))
            {
                string resolvedString = _configuration.GetConnectionStringOrSetting(unresolvedConnectionString);

                if (string.IsNullOrEmpty(resolvedString))
                {
                    throw new InvalidOperationException($"Unable to resolve app setting for property '{nameof(CosmosDBMongoTriggerAttribute)}.{propertyName}'. Make sure the app setting exists and has a valid value.");
                }

                return resolvedString;
            }

            throw new ArgumentNullException(propertyName);
        }
    }
}
