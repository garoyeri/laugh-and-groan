namespace LaughAndGroan.Actions
{
    using Amazon.DynamoDBv2.DataModel;

    [DynamoDBTable("Posts", lowerCamelCaseProperties: true)]
    public class PostData
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public string PostId { get; set; }

        public string Url { get; set; }
    }
}