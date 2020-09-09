namespace LaughAndGroan.Actions.Users
{
    using Amazon.DynamoDBv2.DataModel;

    [DynamoDBTable("Users", lowerCamelCaseProperties: true)]
    public class UserData
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("UserEmail")]
        public string UserName { get; set; }
    }
}