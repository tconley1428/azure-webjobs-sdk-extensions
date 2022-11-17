using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Cosmos.ChangeProcessor.Mongo
{
    public class MongoProcessor : IProcessor<MongoLease, BsonDocument>
    {
        private readonly IMongoCollection<BsonDocument> monitoredCollection;
        private readonly Func<BsonDocument, Task> process;

        public MongoProcessor(IMongoCollection<BsonDocument> monitoredCollection, Func<BsonDocument, Task> process)
        {
            this.monitoredCollection = monitoredCollection;
            this.process = process;
        }

        private PipelineDefinition<ChangeStreamDocument<BsonDocument>, BsonDocument> WatchPipeline()
        {
            return new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
                .Match(new BsonDocument(new Dictionary<string, BsonDocument>(){
                            {"operationType", new BsonDocument("$in", new BsonArray(new List<string>(){"insert", "update", "replace" }))}
                        }))
                .Project(new BsonDocument(new Dictionary<string, bool>()
                {
                    { "_id", true },
                    { "fullDocument", true },
                    { "ns", true },
                    { "documentKey", true }
                }));
        }

        public async Task<BsonDocument> ProcessAsync(MongoLease lease, CancellationToken cancellationToken, Action<TimeSpan> delay, Action<BsonDocument> checkpoint)
        {
            ChangeStreamOptions options = new ChangeStreamOptions()
            {
                ResumeAfter = lease.Continuation(),
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
                MaxAwaitTime = TimeSpan.FromSeconds(5)
            };
            IChangeStreamCursor<BsonDocument> cursor = await this.monitoredCollection.WatchAsync(WatchPipeline(), options, cancellationToken);

            bool hasMoreResults;
            do
            {
                hasMoreResults = await cursor.MoveNextAsync(cancellationToken);
                await cursor.Current.ForEachAsync(process, null, cancellationToken);
                checkpoint(cursor.GetResumeToken());
            } while (hasMoreResults && !cancellationToken.IsCancellationRequested);

            return cursor.GetResumeToken();
        }
    }
}
