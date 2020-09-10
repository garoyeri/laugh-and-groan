namespace LaughAndGroan.Actions.Posts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.Lambda.APIGatewayEvents;
    using Amazon.Lambda.Core;
    using Amazon.Lambda.Serialization.SystemTextJson;

    public class PostsHandler
    {
        readonly PostsService _posts = new PostsService();
        readonly ILambdaSerializer _serializer = new CamelCaseLambdaJsonSerializer();

        public async Task<APIGatewayHttpApiV2ProxyResponse> Create(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var token = request.GetAuthorization();
            if (token == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }
            
            var requestData = _serializer.DeserializeObject<PostApiRequest>(request.Body);
            var postCreated = await _posts.CreatePost(token.Subject, requestData.Url);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Body = _serializer.SerializeObject(postCreated),
                StatusCode = 200
            };
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> Get(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            if (request.PathParameters.TryGetValue("postId", out var postId))
            {
                var postFound = await _posts.GetPost(postId);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    },
                    Body = _serializer.SerializeObject(postFound),
                    StatusCode = 200
                };
            }

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 404
            };
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> Delete(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var token = request.GetAuthorization();
            if (token == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }

            var userId = token.Subject;

            if (request.PathParameters.TryGetValue("postId", out var postId))
            {
                if (!await _posts.DeletePost(userId, postId))
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 403
                    };
                }
            }

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 204
            };
        }
    }
}