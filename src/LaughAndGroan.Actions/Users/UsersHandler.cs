namespace LaughAndGroan.Actions.Users
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.Lambda.APIGatewayEvents;
    using Amazon.Lambda.Core;
    using Amazon.Lambda.Serialization.SystemTextJson;

    public class UsersHandler
    {
        readonly UsersService _users = new UsersService();
        readonly ILambdaSerializer _serializer = new CamelCaseLambdaJsonSerializer();

        public async Task<APIGatewayHttpApiV2ProxyResponse> GetMe(APIGatewayHttpApiV2ProxyRequest request,
            ILambdaContext context)
        {
            context.Logger.LogLine("DEBUG " + _serializer.SerializeObject(request));

            var claims = request.GetAuthorization();
            if (claims == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }

            try
            {
                var profile = await _users.GetById(claims["sub"]);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 200,
                    Headers = new Dictionary<string, string>
                    {
                        {"Content-Type", "application/json"}
                    },
                    Body = _serializer.SerializeObject(new UserApiData
                    {
                        UserName = profile.UserName,
                        Id = profile.UserId
                    })
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("ERROR " + e);
                throw;
            }
        }
    }
}
