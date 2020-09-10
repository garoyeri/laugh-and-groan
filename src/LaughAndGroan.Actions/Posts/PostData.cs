namespace LaughAndGroan.Actions.Posts
{
    using System;
    using System.Collections.Generic;
    using Amazon.DynamoDBv2.DataModel;

    [DynamoDBTable("Posts", lowerCamelCaseProperties: true)]
    public class PostData
    {
        [DynamoDBHashKey]
        [DynamoDBGlobalSecondaryIndexRangeKey("UserIdIndex", "ChronologicalPostsIndex")]
        public string PostId { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("UserIdIndex")]
        public string UserId { get; set; }
        
        public string Url { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("ChronologicalPostsIndex")]
        public string Type { get; set; }

        public int? VersionNumber { get; set; }

        [DynamoDBProperty]
        public List<PostReaction> Reactions { get; set; }
    }

    public class PostReaction
    {
        public string UserId { get; set; }

        public string Reaction { get; set; }

        public DateTimeOffset WhenUpdated { get; set; }
    }
}