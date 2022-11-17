namespace Microsoft.Azure.Cosmos.ChangeProcessor.Mongo
{
    using MongoDB.Bson;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class MongoPartition : IPartition
    {
        public MongoPartition(BsonDocument resumeToken)
        {
            ResumeToken = resumeToken;
        }

        public BsonDocument ResumeToken { get; }
    }
}
