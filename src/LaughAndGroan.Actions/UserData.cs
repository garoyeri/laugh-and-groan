namespace LaughAndGroan.Actions
{
    using Amazon.DynamoDBv2.DataModel;

    [DynamoDBTable("Users", lowerCamelCaseProperties: true)]
    public class UserData
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }

        public string UserName { get; set; }
    }
}