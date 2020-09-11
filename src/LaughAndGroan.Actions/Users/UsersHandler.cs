namespace LaughAndGroan.Actions.Users
{
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
            var token = request.GetAuthorization();
            if (token == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }

            var profile = await _users.GetById(token.Subject);

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
    }
}
