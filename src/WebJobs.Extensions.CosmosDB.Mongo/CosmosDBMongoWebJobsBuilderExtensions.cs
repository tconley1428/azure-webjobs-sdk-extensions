// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB.Mongo;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class CosmosDBMongoWebJobsBuilderExtensions
    {
        public static IWebJobsBuilder AddCosmosDBMongo(this IWebJobsBuilder builder)
        {
            builder.AddExtension<CosmosDBMongoExtensionConfigProvider>();
            builder.Services.AddSingleton<ICosmosDBMongoServiceFactory, DefaultCosmosDBMongoServiceFactory>();
            return builder;
        }
    }
}
