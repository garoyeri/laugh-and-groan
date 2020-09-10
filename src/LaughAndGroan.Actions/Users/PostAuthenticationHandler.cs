namespace LaughAndGroan.Actions.Users
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;

    public class PostAuthenticationHandler
    {
        readonly UsersService _users = new UsersService();

        public async Task<JsonElement> CreateUserAfterAuthentication(JsonElement request, ILambdaContext context)
        {
            // example request input
            /*
                {
                  "version": "string",
                  "triggerSource": "string",
                  "region": "AWSRegion",
                  "userPoolId": "string",
                  "userName": "string",
                  "callerContext": {
                    "awsSdkVersion": "string",
                    "clientId": "string"
                  },
                  "request": {
                    "userAttributes": {
                      "string": "string"
                    },
                    "newDeviceUsed": true,
                    "clientMetadata": {
                      "string": "string"
                    }
                  },
                  "response": {}
                }
            */
            
            // TODO: create user

            return request;
        }
    }
}
