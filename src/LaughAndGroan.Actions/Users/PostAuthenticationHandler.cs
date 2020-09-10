namespace LaughAndGroan.Actions.Users
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Amazon.Lambda.Serialization.SystemTextJson;

    public class PostAuthenticationHandler
    {
        readonly UsersService _users = new UsersService();
        readonly ILambdaSerializer _serializer = new CamelCaseLambdaJsonSerializer();

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
            
            context.Logger.LogLine("Received request: " + JsonSerializer.Serialize(request));

            return request;
        }
    }
}
