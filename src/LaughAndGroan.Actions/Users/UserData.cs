namespace LaughAndGroan.Actions.Users
{
    using Amazon.DynamoDBv2.DataModel;

    [DynamoDBTable("Users", lowerCamelCaseProperties: true)]
    public class UserData
    {
        [DynamoDBHashKey]
        public string UserName { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("UserIdIndex")]
        public string UserId { get; set; }
        
        [DynamoDBVersion]
        public int? VersionNumber { get; set; }
    }
}