namespace LaughAndGroan.Actions.Posts
{
    using System.Collections.Generic;
    using System.Linq;
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
            var token = request.GetAuthorization();
            if (token == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }

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

        public async Task<APIGatewayHttpApiV2ProxyResponse> GetPosts(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var token = request.GetAuthorization();
            if (token == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }

            request.QueryStringParameters.TryGetValue("by", out var userName);
            request.QueryStringParameters.TryGetValue("from", out var fromPostId);

            var result = await _posts.GetPosts(fromPostId, userName == null ? null : new[] { userName });
            var response = new GetPostsResponse()
            {
                Data = result.Take(25).Select(p => new PostApiResponse
                {
                    AuthorId = p.UserId,
                    Url = p.Url,
                    Id = p.PostId
                }).ToArray()
            };

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                StatusCode = 200,
                Body = _serializer.SerializeObject(response)
            };
        }
    }
}