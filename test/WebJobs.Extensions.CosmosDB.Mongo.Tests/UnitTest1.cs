// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Microsoft.Azure.Cosmos.WebJobs.Mongo.Tests
{
    public class UnitTest1
    {
        private const string DatabaseName = "TestDatabase";
        private const string CollectionName = "TestCollection";
        private const string ConnectionString = "";

        [Fact]
        public async Task TestMethod1()
        {
            string collection = "test";

            var monitoredContainerMock = new Mock<IMongoCollection<BsonDocument>>(MockBehavior.Strict);
            var monitoredDatabaseMock = new Mock<IMongoDatabase>(MockBehavior.Strict);

            monitoredDatabaseMock
                .Setup(m => m.GetCollection<BsonDocument>(It.Is<string>(s => s == collection), It.IsAny<MongoCollectionSettings>()))
                .Returns(monitoredContainerMock.Object);

            var serviceMock = new Mock<MongoClient>(MockBehavior.Strict);
            serviceMock.Setup(m => m.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>())).Returns(monitoredDatabaseMock.Object);

            var factoryMock = new Mock<ICosmosDBMongoServiceFactory>(MockBehavior.Strict);
            factoryMock.Setup(f => f.CreateService(It.IsAny<string>())).Returns(serviceMock.Object);

            await RunTestAsync(factoryMock.Object, "Client");
        }

        private async Task RunTestAsync(ICosmosDBMongoServiceFactory factory, string testName)
        {
            IHost host = new HostBuilder()
                .ConfigureWebJobs(builder =>
                {
                    builder.AddCosmosDBMongo();
                })
                .ConfigureAppConfiguration(c =>
                {
                    c.Sources.Clear();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICosmosDBMongoServiceFactory>(factory);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                })
            .Build();

            await host.StartAsync();
            await ((JobHost)host.Services.GetService<IJobHost>()).CallAsync(typeof(TestFunctions).GetMethod(testName), null);
            await host.StopAsync();
        }

        public static class TestFunctions
        {
            [NoAutomaticTrigger]
            public static void Client(
                [CosmosDBMongo(ConnectionString, DatabaseName, CollectionName)] MongoClient client)
            {
            }
        }
    }
}