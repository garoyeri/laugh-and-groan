namespace LaughAndGroan.Actions.Posts
{
    using System;
    using System.Collections.Generic;
    using Amazon.DynamoDBv2.DataModel;

    [DynamoDBTable("Posts", lowerCamelCaseProperties: true)]
    public class PostData
    {
        [DynamoDBHashKey]
        public string PostId { get; set; }

        [DynamoDBRangeKey]
        public string PostIdRange { get; set; }

        public string UserId { get; set; }
        
        public string Url { get; set; }

        [DynamoDBVersion]
        public int? VersionNumber { get; set; }

        public List<PostReaction> Reactions { get; set; }
    }

    public class PostReaction
    {
        public string UserId { get; set; }

        public string Reaction { get; set; }

        public DateTimeOffset WhenUpdated { get; set; }
    }
}