namespace Microsoft.Azure.Cosmos.ChangeProcessor.Mongo
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MongoPartitioner : IPartitioner<MongoPartition>
    {
        private readonly IMongoDatabase database;
        private readonly string collection;

        // Start time
        public MongoPartitioner(IMongoDatabase database, string collection)
        {
            this.database = database;
            this.collection = collection;
        }

        public async Task<IEnumerable<MongoPartition>> GetPartitionsAsync()
        {
            BsonDocument result = await this.database.RunCommandAsync<BsonDocument>(new BsonDocument(new Dictionary<string, string>()
            {
                { "customAction", "GetChangeStreamTokens" },
                { "collection", this.collection },
                // Start time
            }));
            return result["resumeAfterTokens"].AsBsonArray.Select(value => new MongoPartition(value.AsBsonDocument));
        }
    }
}
